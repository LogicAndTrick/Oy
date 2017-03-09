namespace LogicAndTrick.Oy
{
    public class Message
    {
        public string Name { get; internal set; }
        public Volume Volume { get; internal set; }
        public object Object { get; set; }

        public Message(string name, object obj, Volume volume)
        {
            Name = name;
            Volume = volume;
            Object = obj;
        }

        public void Consume()
        {
            if (this.Volume == Volume.Normal) {
                this.Volume = Volume.Low;
            }
        }
    }
}
