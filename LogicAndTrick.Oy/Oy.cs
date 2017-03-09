using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy
{
    public static class Oy
    {
        private static Lazy<IMessageBus> _instance;

        static Oy()
        {
            _instance = new Lazy<IMessageBus>(() => new InMemoryMessageBus());
        }

        public static void Use(IMessageBus bus)
        {
            _instance = new Lazy<IMessageBus>(() => bus);
        }

        public static Task Publish<T>(string name, T data, Volume volume = Volume.Normal, CancellationToken token = default(CancellationToken)) where T: class
        {
            return _instance.Value.Publish(name, data, volume, token);
        }

        public static Subscription Subscribe<T>(string name, Action<T> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            return Subscribe<T>(name, (o, m, t) => {
                callback.Invoke(o);
                return Task.FromResult(0);
            });
        }

        public static Subscription Subscribe<T>(string name, Func<T, Task> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            return Subscribe<T>(name, async (o, m, t) => await callback.Invoke(o));
        }

        public static Subscription Subscribe<T>(string name, Func<T, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            return Subscribe<T>(name, async (o, m, t) => await callback.Invoke(o, t));
        }

        public static Subscription Subscribe<T>(string name, Func<T, Message, CancellationToken, Task> callback, Volume minimumVolume = Volume.Normal) where T : class
        {
            return _instance.Value.Subscribe(name, callback, minimumVolume);
        }

        public static void Unsubscribe(Subscription subscription)
        {
            _instance.Value.Unsubscribe(subscription);
        }
    }
}
