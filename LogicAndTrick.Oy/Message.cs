namespace LogicAndTrick.Oy
{
    /// <summary>
    /// A message object
    /// </summary>
    public class Message
    {
        /// <summary>
        /// The name of the message
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// The volume of the message
        /// </summary>
        public Volume Volume { get; private set; }

        /// <summary>
        /// The message data
        /// </summary>
        public object Object { get; set; }

        /// <summary>
        /// Create a message
        /// </summary>
        /// <param name="name"></param>
        /// <param name="obj"></param>
        /// <param name="volume"></param>
        public Message(string name, object obj, Volume volume)
        {
            Name = name;
            Volume = volume;
            Object = obj;
        }

        /// <summary>
        /// Consume this message by reducing the volume
        /// </summary>
        public void Consume()
        {
            if (Volume == Volume.Normal)
            {
                Volume = Volume.Low;
            }
        }
    }
}
