namespace Umbraco.Web.PublishedCache.NuCache.Snap
{
    internal class LinkedNode<TValue>
        where TValue : class
    {
        public LinkedNode(TValue value, long gen, LinkedNode<TValue> next = null)
        {
            Value = value;
            Gen = gen;
            Next = next;
        }

        public readonly long Gen;

        // reading & writing references is thread-safe on all .NET platforms
        // mark as volatile to ensure we always read the correct value
        public volatile TValue Value;
        public volatile LinkedNode<TValue> Next;
    }
}