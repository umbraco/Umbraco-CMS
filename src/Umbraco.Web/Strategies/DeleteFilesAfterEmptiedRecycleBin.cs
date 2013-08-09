using Umbraco.Core;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Persistence.Repositories;

namespace Umbraco.Web.Strategies
{
    /// <summary>
    /// Represents the DeleteFilesAfterEmptiedRecycleBin class, which subscribes to the
    /// RecycleBinEmptied event of the <see cref="RecycleBinRepository"/> class
    /// and is responsible for deleting files attached to the items that were
    /// permanently deleted from the Recycle Bin - when the Recycle Bin was emptied.
    /// </summary>
    public sealed class DeleteFilesAfterEmptiedRecycleBin : ApplicationEventHandler
    {
        protected override void ApplicationStarted(UmbracoApplicationBase umbracoApplication,
            ApplicationContext applicationContext)
        {
            RecycleBinRepository.RecycleBinEmptied += RecycleBinRepository_RecycleBinEmptied;
        }

        void RecycleBinRepository_RecycleBinEmptied(RecycleBinRepository sender, RecycleBinEventArgs e)
        {
            var success = sender.DeleteFiles(e.Files);

            if(success)
                LogHelper.Info<RecycleBinRepository>("All files attached to delete nodes were deleted as part of emptying the Recycle Bin");
        }
    }
}