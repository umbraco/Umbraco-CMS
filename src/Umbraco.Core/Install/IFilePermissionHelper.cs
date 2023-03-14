// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.Install;

/// <summary>
///     Helper to test File and folder permissions
/// </summary>
public interface IFilePermissionHelper
{
    /// <summary>
    ///     Run all tests for permissions of the required files and folders.
    /// </summary>
    /// <returns>True if all permissions are correct. False otherwise.</returns>
    bool RunFilePermissionTestSuite(out Dictionary<FilePermissionTest, IEnumerable<string>> report);
}
