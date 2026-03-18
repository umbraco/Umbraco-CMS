using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

/// <summary>
/// Unit tests for the StackQueue collection.
/// </summary>
[TestFixture]
public class StackQueueTests
{
    /// <summary>
    /// Verifies that the <see cref="StackQueue{T}"/> class behaves as a queue by enqueuing a sequence of integers
    /// and asserting that they are dequeued in the same order (FIFO).
    /// </summary>
    [Test]
    public void Queue()
    {
        var sq = new StackQueue<int>();
        for (var i = 0; i < 3; i++)
        {
            sq.Enqueue(i);
        }

        var expected = 0;
        while (sq.Count > 0)
        {
            var next = sq.Dequeue();
            Assert.AreEqual(expected, next);
            expected++;
        }
    }

    /// <summary>
    /// Verifies that the <see cref="StackQueue{T}"/> behaves as a stack (LIFO) when using <c>Push</c> and <c>Pop</c> operations.
    /// Ensures that items are returned in reverse order of insertion.
    /// </summary>
    [Test]
    public void Stack()
    {
        var sq = new StackQueue<int>();
        for (var i = 0; i < 3; i++)
        {
            sq.Push(i);
        }

        var expected = 2;
        while (sq.Count > 0)
        {
            var next = sq.Pop();
            Assert.AreEqual(expected, next);
            expected--;
        }
    }

    /// <summary>
    /// Tests the behavior of the StackQueue by pushing and enqueuing elements,
    /// then verifying the order of elements when popping and dequeuing.
    /// </summary>
    [Test]
    public void Stack_And_Queue()
    {
        var sq = new StackQueue<int>();
        for (var i = 0; i < 5; i++)
        {
            if (i % 2 == 0)
            {
                sq.Push(i);
            }
            else
            {
                sq.Enqueue(i);
            }
        }

        // 4 (push)
        // 3 (enqueue)
        // 2 (push)
        // 1 (enqueue)
        // 0 (push)
        Assert.AreEqual(4, sq.Pop());
        Assert.AreEqual(0, sq.Dequeue());
        Assert.AreEqual(3, sq.Pop());
        Assert.AreEqual(1, sq.Dequeue());
        Assert.AreEqual(2, sq.Pop());
    }
}
