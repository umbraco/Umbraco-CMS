using System.Collections.Generic;

namespace Umbraco.Core.Install
{
    public interface IFilePermissionHelper
    {
        bool RunFilePermissionTestSuite(out Dictionary<string, IEnumerable<string>> report);
        bool EnsureDirectories(string[] dirs, out IEnumerable<string> errors, bool writeCausesRestart = false);
        bool EnsureFiles(string[] files, out IEnumerable<string> errors);
    }
}
