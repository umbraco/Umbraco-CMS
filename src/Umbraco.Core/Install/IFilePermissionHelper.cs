using System.Collections.Generic;

namespace Umbraco.Core.Install
{
    public interface IFilePermissionHelper
    {
        bool RunFilePermissionTestSuite(out Dictionary<string, IEnumerable<string>> report);

        /// <summary>
        /// This will test the directories for write access
        /// </summary>
        /// <param name="dirs">The directories to check</param>
        /// <param name="errors">The resulting errors, if any</param>
        /// <param name="writeCausesRestart">
        /// If this is false, the easiest way to test for write access is to write a temp file, however some folder will cause
        /// an App Domain restart if a file is written to the folder, so in that case we need to use the ACL APIs which aren't as
        /// reliable but we cannot write a file since it will cause an app domain restart.
        /// </param>
        /// <returns>Returns true if test succeeds</returns>
        // TODO: This shouldn't exist, see notes in FolderAndFilePermissionsCheck.GetStatus
        bool EnsureDirectories(string[] dirs, out IEnumerable<string> errors, bool writeCausesRestart = false);

        // TODO: This shouldn't exist, see notes in FolderAndFilePermissionsCheck.GetStatus
        bool EnsureFiles(string[] files, out IEnumerable<string> errors);
    }
}
