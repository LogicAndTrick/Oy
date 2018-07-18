using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy
{
    /// <summary>
    /// A message bus interface
    /// </summary>
    public interface IMessageBus
    {
        /// <summary>
        /// Publish a message to the bus
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="obj"></param>
        /// <param name="volume"></param>
        /// <param name="token"></param>
        /// <returns></returns>
        Task Publish<T>(string name, T obj, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken));

        /// <summary>
        /// Subscribe to this messagte bus
        /// </summary>
        /// <typeparam name="T">Data type</typeparam>
        /// <param name="name">Message name</param>
        /// <param name="callback"></param>
        /// <param name="minimumVolume"></param>
        /// <returns></returns>
        Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal);

        /// <summary>
        /// Unsubscribe from this message bus
        /// </summary>
        /// <param name="subscription">Subscription to remove</param>
        void Unsubscribe(Subscription subscription);

        /// <summary>
        /// Triggered when an exception is unhandled while processing a message
        /// </summary>
        event UnhandledExceptionEventHandler UnhandledException;
    }
}
