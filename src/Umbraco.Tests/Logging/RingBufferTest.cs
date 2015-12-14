using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using NUnit.Framework;
using Umbraco.Core.Logging;
using Umbraco.Tests.TestHelpers;

namespace Umbraco.Tests.Logging
{
    [TestFixture]
    public class RingBufferTest
    {
        [Test]
        public void PerfTest()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);
            Stopwatch ringWatch = new Stopwatch();
            ringWatch.Start();
            for (int i = 0; i < 1000000; i++)
            {
                ringBuffer.Enqueue("StringOfFun");
            }
            ringWatch.Stop();

            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(150));
        }

        [Test]
        public void PerfTestThreads()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);

            Stopwatch ringWatch = new Stopwatch();
            List<Task> ringTasks = new List<Task>();
            CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
            CancellationToken cancelationToken = cancelationTokenSource.Token;
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        ringBuffer.Enqueue("StringOfFun");
                    }
                }, cancelationToken));
            }
            ringWatch.Start();
            ringTasks.ForEach(t => t.Start());
            var allTasks = ringTasks.ToArray();
            Task.WaitAny(allTasks);
            ringWatch.Stop();
            //Cancel tasks to avoid System.AppDominUnloadException which is caused when the domain is unloaded
            //and threads created in the domain are not stopped.
            //Do this before assertions because they may throw an exception causing the thread cancellation to not happen.
            cancelationTokenSource.Cancel();
            try
            {
                Task.WaitAll(allTasks);
            }
            catch (AggregateException)
            {
                //Don't care about cancellation Exceptions.
            }
            //Tolerance at 500 was too low
            ringTasks.ForEach(t => t.Dispose());
            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(1000));
        }

        [Test]
        public void PerfTestThreadsWithDequeues()
        {
            RingBuffer<string> ringBuffer = new RingBuffer<string>(1000);

            Stopwatch ringWatch = new Stopwatch();
            List<Task> ringTasks = new List<Task>();
            CancellationTokenSource cancelationTokenSource = new CancellationTokenSource();
            CancellationToken cancelationToken = cancelationTokenSource.Token;
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        ringBuffer.Enqueue("StringOfFun");
                    }
                }, cancelationToken));
            }
            for (int t = 0; t < 10; t++)
            {
                ringTasks.Add(new Task(() =>
                {
                    for (int i = 0; i < 1000000; i++)
                    {
                        string foo;
                        ringBuffer.TryDequeue(out foo);
                    }
                }));
            }
            ringWatch.Start();
            ringTasks.ForEach(t => t.Start());
            var allTasks = ringTasks.ToArray();
            Task.WaitAny(allTasks);
            ringWatch.Stop();
            //Cancel tasks to avoid System.AppDominUnloadException which is caused when the domain is unloaded
            //and threads created in the domain are not stopped.
            //Do this before assertions because they may throw an exception causing the thread cancellation to not happen.
            cancelationTokenSource.Cancel();
            try
            {
                Task.WaitAll(allTasks);
            }
            catch (AggregateException)
            {
                //Don't care about cancellation Exceptions.
            }
            ringTasks.ForEach(t => t.Dispose());
            Assert.That(ringWatch.ElapsedMilliseconds, Is.LessThan(800));
        }

        [Test]
        public void WhenRingSizeLimitIsHit_ItemsAreDequeued()
        {
            // Arrange
            const int limit = 2;
            object object1 = "one";
            object object2 = "two";
            object object3 = "three";
            RingBuffer<object> queue = new RingBuffer<object>(limit);

            // Act
            queue.Enqueue(object1);
            queue.Enqueue(object2);
            queue.Enqueue(object3);

            // Assert
            object value;
            queue.TryDequeue(out value);
            Assert.That(value, Is.EqualTo(object2));
            queue.TryDequeue(out value);
            Assert.That(value, Is.EqualTo(object3));
            queue.TryDequeue(out value);
            Assert.That(value, Is.Null);
        }
    }
}