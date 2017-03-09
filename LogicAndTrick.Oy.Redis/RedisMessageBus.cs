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
        private ConnectionMultiplexer _connection;
        private ConcurrentDictionary<Subscription, Action<RedisChannel, RedisValue>> _handlers;

        public RedisMessageBus(ConnectionMultiplexer connection)
        {
            _connection = connection;
            _handlers = new ConcurrentDictionary<Subscription, Action<RedisChannel, RedisValue>>();
        }

        public async Task Publish<T>(string name, T obj, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken)) where T : class
        {
            var sub = _connection.GetSubscriber();
            var message = new Message(name, JsonConvert.SerializeObject(obj), volume);
            await sub.PublishAsync(name, JsonConvert.SerializeObject(message));
        }

        public Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            var subscription = new Subscription(name, async (o, m, t) => {
                if (o is T) await callback.Invoke((T) o, m, t);
            }, minimumVolume);

            Action<RedisChannel, RedisValue> handler = (c, v) => {
                var msg = JsonConvert.DeserializeObject<Message>(v);
                try {
                    msg.Object = JsonConvert.DeserializeObject<T>(msg.Object as string);
                    subscription.Invoke(msg.Object, msg, CancellationToken.None);
                } catch (JsonSerializationException) {
                    // Not a valid object
                }
            };

            if (!_handlers.TryAdd(subscription, handler)) {
                throw new Exception("Unable to subscribe");
            }

            var sub = _connection.GetSubscriber();
            sub.Subscribe(name, handler);

            return subscription;
        }

        public void Unsubscribe(Subscription subscription)
        {
            Action<RedisChannel, RedisValue> act;
            if (_handlers.TryRemove(subscription, out act)) {
                var sub = _connection.GetSubscriber();
                sub.Unsubscribe(subscription.Name, act);
            }
        }
    }
}
