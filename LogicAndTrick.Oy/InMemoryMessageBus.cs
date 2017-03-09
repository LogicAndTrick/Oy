using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;

namespace LogicAndTrick.Oy
{
    // This implementation is heavily inspired by:
    // https://github.com/benfoster/Fabrik.SimpleBus
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<Subscription, bool> _subscriptions = new ConcurrentDictionary<Subscription, bool>();
        private readonly ConcurrentQueue<Subscription> _pendingSubscriptions = new ConcurrentQueue<Subscription>();
        private readonly ConcurrentQueue<Subscription> _pendingUnsubscriptions = new ConcurrentQueue<Subscription>();

        public InMemoryMessageBus()
        {
            
        }

        public async Task Publish<T>(string name, T obj, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken)) where T : class
        {
            var message = new Message(name, obj, volume);
            // Process subscriptions and unsubscriptions
            Subscription sub;
            bool b;
            while (_pendingUnsubscriptions.TryDequeue(out sub)) _subscriptions.TryRemove(sub, out b);
            while (_pendingSubscriptions.TryDequeue(out sub)) _subscriptions.TryAdd(sub, true);
            foreach (var subscription in _subscriptions.Keys.ToList())
            {
                if (token.IsCancellationRequested)
                {
                    break;
                }

                if (!subscription.ShouldPost(message))
                {
                    continue;
                }
                
                try
                {
                    await subscription.Invoke(obj, message, token);
                }
                catch
                {
                    continue;
                }
            }
        }

        public Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            var sub = new Subscription(name, async (o, m, t) => {
                if (o is T) await callback.Invoke((T) o, m, t);
            }, minimumVolume);
            _pendingSubscriptions.Enqueue(sub);
            return sub;
        }

        public void Unsubscribe(Subscription subscription)
        {
            _pendingUnsubscriptions.Enqueue(subscription);
        }
    }
}
