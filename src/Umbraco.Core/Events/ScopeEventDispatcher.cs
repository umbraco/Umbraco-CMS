using Umbraco.Core.IO;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// This event manager is created for each scope and is aware of if it is nested in an outer scope
    /// </summary>
    /// <remarks>
    /// The outer scope is the only scope that can raise events, the inner scope's will defer to the outer scope
    /// </remarks>
    internal class ScopeEventDispatcher : ScopeEventDispatcherBase
    {
        public ScopeEventDispatcher()
            : base(true)
        { }

        protected override void ScopeExitCompleted()
        {
            // fixme - we'd need to de-duplicate events somehow, etc - and the deduplication should be last in wins

            foreach (var e in GetEvents(EventDefinitionFilter.All))
            {
                e.RaiseEvent();

                // fixme - not sure I like doing it here - but then where? how?
                var delete = e.Args as IDeletingMediaFilesEventArgs;
                if (delete != null && delete.MediaFilesToDelete.Count > 0)
                    MediaFileSystem.DeleteMediaFiles(delete.MediaFilesToDelete);
            }
        }

        private MediaFileSystem _mediaFileSystem;

        private MediaFileSystem MediaFileSystem
        {
            get
            {
                if (_mediaFileSystem != null) return _mediaFileSystem;

                // fixme - insane! reading config goes cross AppDomain and serializes context?
                using (new SafeCallContext())
                {
                    return _mediaFileSystem = FileSystemProviderManager.Current.MediaFileSystem;
                }
            }
        }
    }
}