using System;
using System.Collections.Concurrent;
using System.Linq;

namespace Umbraco.Core.ObjectResolution
{
    /// <summary>
    /// Simply used to track all ManyObjectsResolverBase instances so that we can 
    /// reset them all at once really easily. 
    /// </summary>
    /// <remarks>
    /// Normally we'd use TypeFinding for this but because many of the resolvers are internal this won't work.
    /// We'd rather not keep a static list of them so we'll dynamically add to this list based on the base
    /// class of the ManyObjectsResolverBase.
    /// </remarks>
    internal static class ResolverCollection
    {
        private static readonly ConcurrentDictionary<ResolverBase, Action> Resolvers = new ConcurrentDictionary<ResolverBase, Action>();

        /// <summary>
        /// Returns the number of resolvers created
        /// </summary>
        internal static int Count
        {
            get { return Resolvers.Count; }
        }

        /// <summary>
        /// Resets all resolvers
        /// </summary>
        internal static void ResetAll()
        {
            //take out each item from the bag and reset it
            var keys = Resolvers.Keys.ToArray();
            foreach (var k in keys)
            {
                Action resetAction;
                while (Resolvers.TryRemove(k, out resetAction))
                {
                    //call the reset action for the resolver
                    resetAction();
                }
            }
        }

        /// <summary>
        /// This is called when the static Reset method or a ResolverBase{T} is called.
        /// </summary>
        internal static void Remove(ResolverBase resolver)
        {
            if (resolver == null) return;
            Action action;
            Resolvers.TryRemove(resolver, out action);
        }

        /// <summary>
        /// Adds a resolver to the collection
        /// </summary>
        /// <param name="resolver"></param>
        /// <param name="resetAction"></param>
        /// <remarks>
        /// This is called when the creation of a ResolverBase occurs
        /// </remarks>
        internal static void Add(ResolverBase resolver, Action resetAction)
        {
            Resolvers.TryAdd(resolver, resetAction);
        }
    }
}