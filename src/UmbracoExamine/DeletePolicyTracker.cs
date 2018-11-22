using System.Collections.Concurrent;
using System.IO;
using Lucene.Net.Index;

namespace UmbracoExamine
{
    internal sealed class DeletePolicyTracker
    {
        private static readonly DeletePolicyTracker Instance = new DeletePolicyTracker();
        private readonly ConcurrentDictionary<string, IndexDeletionPolicy> _directories = new ConcurrentDictionary<string, IndexDeletionPolicy>();

        public static DeletePolicyTracker Current
        {
            get { return Instance; }
        }

        public IndexDeletionPolicy GetPolicy(Lucene.Net.Store.Directory directory)
        {
            var resolved = _directories.GetOrAdd(directory.GetLockID(), s => new SnapshotDeletionPolicy(new KeepOnlyLastCommitDeletionPolicy()));
            return resolved;
        }
    }
}