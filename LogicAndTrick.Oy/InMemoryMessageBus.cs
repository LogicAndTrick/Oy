using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using System.Linq;
using System.Collections.Generic;

namespace LogicAndTrick.Oy
{
    // This implementation is heavily inspired by:
    // https://github.com/benfoster/Fabrik.SimpleBus
    /// <summary>
    /// A simple in-memory message bus implementation
    /// </summary>
    public class InMemoryMessageBus : IMessageBus
    {
        private readonly ConcurrentDictionary<Subscription, bool> _subscriptions = new ConcurrentDictionary<Subscription, bool>();
        private readonly ConcurrentQueue<Subscription> _pendingSubscriptions = new ConcurrentQueue<Subscription>();
        private readonly ConcurrentQueue<Subscription> _pendingUnsubscriptions = new ConcurrentQueue<Subscription>();

        /// <summary>
        /// Construct a message bus
        /// </summary>
        public InMemoryMessageBus()
        {
            
        }

        /// <inheritdoc />
        public async Task Publish<T>(string name, T obj, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken))
        {
            var message = new Message(name, obj, volume);
            // Process subscriptions and unsubscriptions
            Subscription sub;
            bool b;
            while (_pendingUnsubscriptions.TryDequeue(out sub)) _subscriptions.TryRemove(sub, out b);
            while (_pendingSubscriptions.TryDequeue(out sub)) _subscriptions.TryAdd(sub, true);

            var done = Task.FromResult(0);
            var list = new List<Task> { done };

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
                    list.Add(subscription.Invoke(obj, message, token));
                }
                catch
                {
                    continue;
                }
            }
            await Task.WhenAll(list);
        }

        /// <inheritdoc />
        public Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            var sub = new Subscription(name, async (o, m, t) => {
                if (o is T variable) await callback.Invoke(variable, m, t);
            }, minimumVolume, Unsubscribe);
            _pendingSubscriptions.Enqueue(sub);
            return sub;
        }

        /// <inheritdoc />
        public void Unsubscribe(Subscription subscription)
        {
            _pendingUnsubscriptions.Enqueue(subscription);
        }
    }
}
