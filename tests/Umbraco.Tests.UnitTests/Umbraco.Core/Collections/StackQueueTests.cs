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
            Assert.That(next, Is.EqualTo(expected));
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
            Assert.That(next, Is.EqualTo(expected));
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
        Assert.That(sq.Pop(), Is.EqualTo(4));
        Assert.That(sq.Dequeue(), Is.EqualTo(0));
        Assert.That(sq.Pop(), Is.EqualTo(3));
        Assert.That(sq.Dequeue(), Is.EqualTo(1));
        Assert.That(sq.Pop(), Is.EqualTo(2));
    }
}
