// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.IO;

namespace Umbraco.Cms.Core.Events;

/// <summary>
///     An IEventDispatcher that queues events, and raise them when the scope
///     exits and has been completed.
/// </summary>
public class QueuingEventDispatcher : QueuingEventDispatcherBase
{
    private readonly MediaFileManager _mediaFileManager;

    public QueuingEventDispatcher(MediaFileManager mediaFileManager)
        : base(true) =>
        _mediaFileManager = mediaFileManager;

    protected override void ScopeExitCompleted()
    {
        // processing only the last instance of each event...
        // this is probably far from perfect, because if eg a content is saved in a list
        // and then as a single content, the two events will probably not be de-duplicated,
        // but it's better than nothing
        foreach (IEventDefinition e in GetEvents(EventDefinitionFilter.LastIn))
        {
            e.RaiseEvent();

            // separating concerns means that this should probably not be here,
            // but then where should it be (without making things too complicated)?
            if (e.Args is IDeletingMediaFilesEventArgs delete && delete.MediaFilesToDelete.Count > 0)
            {
                _mediaFileManager.DeleteMediaFiles(delete.MediaFilesToDelete);
            }
        }
    }
}
