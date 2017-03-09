using System;

namespace LogicAndTrick.Oy.Demo
{
    public class Program
    {
        static void Main(string[] args)
        {
            Oy.Subscribe<Something>("Message", async (x, t) => Console.WriteLine(x.Number + " - " + x.Message));

            for (var i = 5; i >= 0; i--)
            {
                Oy.Publish("Message", new Something { Number = i, Message = "Message #" + i });
                Oy.Publish("Message", "Nonsense");
            }
            System.Threading.Thread.Sleep(2000);
        }
    }

    public class Something
    {
        public string Message { get; set; }
        public int Number { get; set; }
    }
}
