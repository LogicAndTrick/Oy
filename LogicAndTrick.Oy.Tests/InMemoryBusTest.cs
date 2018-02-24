using System;
using Xunit;
using System.Threading;
using System.Threading.Tasks;

namespace LogicAndTrick.Oy.Tests
{
    public class InMemoryBusTest
    {
        [Fact]
        public void TestThatItWorks()
        {
            var tcs = new TaskCompletionSource<string>();

            Oy.Subscribe<string>("Test it works", x => {
                tcs.SetResult(x);
            });

            Oy.Publish("Test it works", "Value");

            tcs.Task.Wait(100);
            Assert.Equal("Value", tcs.Task.Result);
        }

        [Fact]
        public void TestThatMultipleTypesAreSupportedOnTheSameChannel()
        {
            var tcs1 = new TaskCompletionSource<string>();
            var tcs2 = new TaskCompletionSource<Tuple<int, string>>();

            Oy.Subscribe<string>("Test MultiType", x => tcs1.SetResult(x));
            Oy.Subscribe<Tuple<int, string>>("Test MultiType", x => tcs2.SetResult(x));

            Oy.Publish("Test MultiType", "Value");
            Oy.Publish("Test MultiType", Tuple.Create(1, "One"));

            Task.WaitAll(new Task[] {tcs1.Task, tcs2.Task}, 100);
            Assert.Equal("Value", tcs1.Task.Result);
            Assert.Equal(Tuple.Create(1, "One"), tcs2.Task.Result);
        }

        [Fact]
        public void TestThatMultipleChannelsAreSupported()
        {
            int hitCount = 0;
            var tcs1 = new TaskCompletionSource<string>();
            var tcs2 = new TaskCompletionSource<string>();
            var tcs3 = new TaskCompletionSource<string>();

            Oy.Subscribe<string>("Test MultiChannel 1", x => { Interlocked.Add(ref hitCount, 1); tcs1.SetResult(x); });
            Oy.Subscribe<string>("Test MultiChannel 2", x => { Interlocked.Add(ref hitCount, 1); tcs2.SetResult(x); });
            Oy.Subscribe<string>("Test MultiChannel 3", x => { Interlocked.Add(ref hitCount, 1); tcs3.SetResult(x); });

            Oy.Publish("Test MultiChannel 1", "Value 1");
            Oy.Publish("Test MultiChannel 2", "Value 2");
            Oy.Publish("Test MultiChannel 3", "Value 3");

            Task.WaitAll(new Task[] {tcs1.Task, tcs2.Task, tcs3.Task}, 100);
            Assert.Equal("Value 1", tcs1.Task.Result);
            Assert.Equal("Value 2", tcs2.Task.Result);
            Assert.Equal("Value 3", tcs3.Task.Result);
            Assert.Equal(3, hitCount);
        }

        [Fact]
        public void TestUpCasting()
        {
            int hitCount = 0;
            var tcs = new TaskCompletionSource<string>();

            Oy.Subscribe<string>("Test Upcasting", x => { Interlocked.Add(ref hitCount, 1); tcs.SetResult(x); });

            Oy.Publish<object>("Test Upcasting", (object)"Value 1");

            tcs.Task.Wait(100);
            Assert.Equal("Value 1", tcs.Task.Result);
            Assert.Equal(1, hitCount);
        }

        [Fact]
        public void TestDownCasting()
        {
            int hitCount = 0;
            var tcs = new TaskCompletionSource<object>();

            Oy.Subscribe<object>("Test Downcasting", x => { Interlocked.Add(ref hitCount, 1); tcs.SetResult(x); });

            Oy.Publish<string>("Test Downcasting", "Value 1");
            
            tcs.Task.Wait(100);
            Assert.Equal("Value 1", tcs.Task.Result);
            Assert.Equal(1, hitCount);
        }

        [Fact]
        public void TestPublishAsync()
        {
            int i = 0;
            Oy.Subscribe<object>("Test Async", async x =>
            {
                await Task.Delay(100);
                i = 1;
            });
            Oy.Subscribe<object>("Test Async", x =>
            {
                i = 2;
            });
            // All subscriptions should run in parallel
            Oy.Publish<object>("Test Async", new object()).Wait(200);
            Assert.Equal(1, i);
        }

        [Fact]
        public void TestStruct()
        {
            int number = 0;
            Oy.Subscribe<decimal>("Test 1", d =>
            {
                Interlocked.Add(ref number, (int) d);
            });
            Oy.Publish("Test 1", 10m).Wait(100);
            Assert.Equal(10, number);
        }

        [Fact]
        public void TestEmpty()
        {
            int number = 0;
            Oy.Subscribe("Nothing", () =>
            {
                Interlocked.Add(ref number, 1);
            });
            Oy.Publish("Nothing").Wait(100);
            Assert.Equal(1, number);
        }
    }
}
