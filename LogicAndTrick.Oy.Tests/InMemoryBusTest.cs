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

            Oy.Subscribe<string>("Test 1", x => {
                tcs.SetResult(x);
            });

            Oy.Publish("Test 1", "Value");

            tcs.Task.Wait(100);
            Assert.Equal("Value", tcs.Task.Result);
        }

        [Fact]
        public void TestThatMultipleTypesAreSupportedOnTheSameChannel()
        {
            var tcs1 = new TaskCompletionSource<string>();
            var tcs2 = new TaskCompletionSource<Tuple<int, string>>();

            Oy.Subscribe<string>("Test 1", x => tcs1.SetResult(x));
            Oy.Subscribe<Tuple<int, string>>("Test 1", x => tcs2.SetResult(x));

            Oy.Publish("Test 1", "Value");
            Oy.Publish("Test 1", Tuple.Create(1, "One"));

            Task.WaitAll(tcs1.Task, tcs2.Task);
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

            Oy.Subscribe<string>("Test 1", x => { Interlocked.Add(ref hitCount, 1); tcs1.SetResult(x); });
            Oy.Subscribe<string>("Test 2", x => { Interlocked.Add(ref hitCount, 1); tcs2.SetResult(x); });
            Oy.Subscribe<string>("Test 3", x => { Interlocked.Add(ref hitCount, 1); tcs3.SetResult(x); });

            Oy.Publish("Test 1", "Value 1");
            Oy.Publish("Test 2", "Value 2");
            Oy.Publish("Test 3", "Value 3");

            Task.WaitAll(tcs1.Task, tcs2.Task, tcs3.Task);
            Assert.Equal("Value 1", tcs1.Task.Result);
            Assert.Equal("Value 2", tcs2.Task.Result);
            Assert.Equal("Value 3", tcs3.Task.Result);
            Assert.Equal(3, hitCount);
        }
    }
}
