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
        Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal);
        void Unsubscribe(Subscription subscription);
    }
}
