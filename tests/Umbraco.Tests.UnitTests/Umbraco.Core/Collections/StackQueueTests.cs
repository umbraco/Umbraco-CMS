using NUnit.Framework;
using Umbraco.Cms.Core.Collections;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Collections;

[TestFixture]
public class StackQueueTests
{
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
