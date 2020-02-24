using System.Collections.Generic;

namespace Umbraco.Core.Install
{
    public interface IFilePermissionHelper
    {
        bool RunFilePermissionTestSuite(out Dictionary<string, IEnumerable<string>> report);
        bool EnsureDirectories(string[] dirs, out IEnumerable<string> errors, bool writeCausesRestart = false);
        bool EnsureFiles(string[] files, out IEnumerable<string> errors);
        bool EnsureCanCreateSubDirectory(string dir, out IEnumerable<string> errors);
        bool EnsureCanCreateSubDirectories(IEnumerable<string> dirs, out IEnumerable<string> errors);
        bool TestPublishedSnapshotService(out IEnumerable<string> errors);
        bool TryCreateDirectory(string dir);
        bool TryAccessDirectory(string dir, bool canWrite);
    }
}
