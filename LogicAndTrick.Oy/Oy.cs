using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy
{
    /// <summary>
    /// A global message bus interface.
    /// </summary>
    public static class Oy
    {
        private static Lazy<IMessageBus> _instance;

        static Oy()
        {
            _instance = new Lazy<IMessageBus>(() => new InMemoryMessageBus());
        }

        /// <summary>
        /// Change the bus used by the global interface.
        /// </summary>
        /// <param name="bus">The message bus to use</param>
        public static void Use(IMessageBus bus)
        {
            _instance = new Lazy<IMessageBus>(() => bus);
        }

        /// <summary>
        /// Publish a message to the global bus.
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="data">Message data</param>
        /// <param name="volume">Message volume</param>
        /// <param name="token">Message cancellation token</param>
        /// <returns>Completion task</returns>
        public static Task Publish<T>(string name, T data, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken))
        {
            return _instance.Value.Publish(name, data, volume, token);
        }

        /// <summary>
        /// Publish a message with no data to the global bus
        /// </summary>
        /// <param name="name">Message name</param>
        /// <param name="volume">Message volume</param>
        /// <param name="token">Message cancellation token</param>
        /// <returns>Completion task</returns>
        public static Task Publish(string name, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken))
        {
            return Publish(name, Nothing.Instance, volume, token);
        }

        /// <summary>
        /// Subscribe to the global bus with a simple action callback
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe<T>(string name, Action<T> callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<T>(name, (o, m, t) => {
                callback.Invoke(o);
                return Task.FromResult(0);
            });
        }

        /// <summary>
        /// Subscribe to the global bus with a simple action callback with no data
        /// </summary>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe(string name, Action callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<Nothing>(name, (o, m, t) => {
                callback.Invoke();
                return Task.FromResult(0);
            });
        }
        
        /// <summary>
        /// Subscribe to the global bus with an async callback
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe<T>(string name, Func<T, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<T>(name, (o, m, t) => callback.Invoke(o));
        }
        
        /// <summary>
        /// Subscribe to the global bus with an async callback with no data
        /// </summary>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe(string name, Func<Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<Nothing>(name, (o, m, t) => callback.Invoke());
        }
        
        /// <summary>
        /// Subscribe to the global bus with a cancellable async callback
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe<T>(string name, Func<T, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<T>(name, (o, m, t) => callback.Invoke(o, t), minimumVolume);
        }
        
        /// <summary>
        /// Subscribe to the global bus with a cancellable async callback with no data
        /// </summary>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe(string name, Func<CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return Subscribe<Nothing>(name, (o, m, t) => callback.Invoke(t), minimumVolume);
        }
        
        /// <summary>
        /// Subscribe to the global bus with a cancellable async callback, including the published message
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return _instance.Value.Subscribe(name, callback, minimumVolume);
        }
        
        /// <summary>
        /// Subscribe to the global bus with a cancellable async callback with no data, including the published message
        /// </summary>
        /// <param name="name">Message name</param>
        /// <param name="callback">Subscription callback</param>
        /// <param name="minimumVolume">The minimum volume for message to trigger this subscription</param>
        /// <returns>Subscription object</returns>
        public static Subscription Subscribe(string name, Func<Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal)
        {
            return _instance.Value.Subscribe<Nothing>(name, (o, m, t) => callback(m, t), minimumVolume);
        }

        /// <summary>
        /// Remove a subscription from the global bus
        /// </summary>
        /// <param name="subscription"></param>
        public static void Unsubscribe(Subscription subscription)
        {
            _instance.Value.Unsubscribe(subscription);
        }
    }
}
