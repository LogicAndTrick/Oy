using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using Newtonsoft.Json;
using StackExchange.Redis;

namespace LogicAndTrick.Oy.Redis
{
    public class RedisMessageBus : IMessageBus
    {
        private readonly ConnectionMultiplexer _connection;
        private readonly ConcurrentDictionary<Subscription, Action<RedisChannel, RedisValue>> _handlers;

        public RedisMessageBus(ConnectionMultiplexer connection)
        {
            _connection = connection;
            _handlers = new ConcurrentDictionary<Subscription, Action<RedisChannel, RedisValue>>();
        }

        public async Task Publish<T>(string name, T obj, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken))
        {
            var sub = _connection.GetSubscriber();
            var message = new Message(name, JsonConvert.SerializeObject(obj), volume);
            await sub.PublishAsync(name, JsonConvert.SerializeObject(message));
        }

        public Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            var subscription = new Subscription(name, async (o, m, t) => {
                if (o is T variable) await callback.Invoke(variable, m, t);
            }, minimumVolume, Unsubscribe);

            if (!_handlers.TryAdd(subscription, Handler)) {
                throw new Exception("Unable to subscribe");
            }

            var sub = _connection.GetSubscriber();
            sub.Subscribe(name, Handler);

            return subscription;

            void Handler(RedisChannel c, RedisValue v)
            {
                try
                {
                    var msg = JsonConvert.DeserializeObject<Message>(v);
                    msg.Object = JsonConvert.DeserializeObject<T>(msg.Object as string);
                    subscription.Invoke(msg.Object, msg, CancellationToken.None);
                }
                catch (Exception ex)
                {
                    UnhandledException?.Invoke(this, new UnhandledExceptionEventArgs(ex));
                }
            }
        }

        public void Unsubscribe(Subscription subscription)
        {
            if (!_handlers.TryRemove(subscription, out var act)) return;
            var sub = _connection.GetSubscriber();
            sub.Unsubscribe(subscription.Name, act);
        }

        /// <inheritdoc />
        public event UnhandledExceptionEventHandler UnhandledException;
    }
}
