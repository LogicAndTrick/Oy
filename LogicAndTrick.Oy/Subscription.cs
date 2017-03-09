using System;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy
{
    public class Subscription
    {
        private Func<object, Message, CancellationToken, Task> _callback;

        public string Name { get; }
        public Guid Guid { get; }
        public Volume MinimumVolume { get; }

        public Subscription(string name, Func<object, Message, CancellationToken, Task> callback, Volume minimumVolume)
        {
            this.Name = name;
            this._callback = callback;
            this.Guid = Guid.NewGuid();
            this.MinimumVolume = minimumVolume;
        }

        public Task Invoke<T>(T obj, Message message, CancellationToken token) where T : class
        {
            return _callback.Invoke(obj, message, token);
        }

        internal bool ShouldPost(Message message)
        {
            return message.Name == this.Name && (int) message.Volume >= (int) this.MinimumVolume;
        }
    }
}
