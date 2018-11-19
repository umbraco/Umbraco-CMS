using Umbraco.Core.Composing;
using Umbraco.Core.IO;

namespace Umbraco.Core.Events
{
    /// <summary>
    /// An IEventDispatcher that queues events, and raise them when the scope
    /// exits and has been completed.
    /// </summary>
    internal class QueuingEventDispatcher : QueuingEventDispatcherBase
    {
        public QueuingEventDispatcher()
            : base(true)
        { }

        protected override void ScopeExitCompleted()
        {
            // processing only the last instance of each event...
            // this is probably far from perfect, because if eg a content is saved in a list
            // and then as a single content, the two events will probably not be de-duplicated,
            // but it's better than nothing

            foreach (var e in GetEvents(EventDefinitionFilter.LastIn))
            {
                e.RaiseEvent();

                // separating concerns means that this should probably not be here,
                // but then where should it be (without making things too complicated)?
                var delete = e.Args as IDeletingMediaFilesEventArgs;
                if (delete != null && delete.MediaFilesToDelete.Count > 0)
                    MediaFileSystem.DeleteMediaFiles(delete.MediaFilesToDelete);
            }
        }

        private IMediaFileSystem _mediaFileSystem;

        // fixme inject
        private IMediaFileSystem MediaFileSystem => _mediaFileSystem ?? (_mediaFileSystem = Current.MediaFileSystem);
    }
}
