namespace Umbraco.Cms.Infrastructure.PublishedCache.Snap;

// NOTE: This cannot be struct because it references itself

/// <summary>
///     Used to represent an item in a linked list
/// </summary>
/// <typeparam name="TValue"></typeparam>
internal class LinkedNode<TValue>
    where TValue : class?
{
    public readonly long Gen;
    public volatile LinkedNode<TValue>? Next;

    // reading & writing references is thread-safe on all .NET platforms
    // mark as volatile to ensure we always read the correct value
    public volatile TValue? Value;

    public LinkedNode(TValue? value, long gen, LinkedNode<TValue>? next = null)
    {
        Value = value; // This is allowed to be null, we actually explicitly set this to null in ClearLocked
        Gen = gen;
        Next = next;
    }
}
