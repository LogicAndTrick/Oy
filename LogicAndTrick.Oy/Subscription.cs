using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy
{
    /// <summary>
    /// A message bus subscription object.
    /// </summary>
    public class Subscription : IDisposable
    {
        private readonly Action<Subscription> _unsubscribeCallback;
        private readonly Func<object, Message, CancellationToken, Task> _callback;

        /// <summary>
        /// The name of the subscription message
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// A unique subscription identifier
        /// </summary>
        public Guid Guid { get; }

        /// <summary>
        /// The subscription's minimum volume
        /// </summary>
        public Volume MinimumVolume { get; }

        /// <summary>
        /// Constructor for a subscription
        /// </summary>
        /// <param name="name">Subscription name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">Minimum volume</param>
        /// <param name="unsubscribeCallback">A callback to remove this subscription</param>
        public Subscription(string name, Func<object, Message, CancellationToken, Task> callback, Volume minimumVolume, Action<Subscription> unsubscribeCallback)
        {
            Name = name;
            _callback = callback;
            Guid = Guid.NewGuid();
            MinimumVolume = minimumVolume;
            _unsubscribeCallback = unsubscribeCallback;
        }

        /// <summary>
        /// Invoke the subscription callback
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="obj">Message data</param>
        /// <param name="message">Message details</param>
        /// <param name="token">Cancellation token</param>
        /// <returns>Async task</returns>
        public Task Invoke<T>(T obj, Message message, CancellationToken token) 
        {
            return _callback.Invoke(obj, message, token);
        }

        internal bool ShouldPost(Message message)
        {
            return message.Name == Name && (int) message.Volume >= (int) MinimumVolume;
        }

        /// <summary>
        /// Remove the subscription from the bus
        /// </summary>
        public void Dispose()
        {
            _unsubscribeCallback(this);
        }
    }
}
