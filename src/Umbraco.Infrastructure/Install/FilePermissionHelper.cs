// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Security.AccessControl;
using System.Security.Principal;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.IO;
using Umbraco.Extensions;

namespace Umbraco.Cms.Infrastructure.Install;

/// <inheritdoc />
public class FilePermissionHelper : IFilePermissionHelper
{
    private readonly GlobalSettings _globalSettings;
    private readonly IHostingEnvironment _hostingEnvironment;
    private readonly IIOHelper _ioHelper;

    private readonly string[] _packagesPermissionsDirs;

    // ensure that these directories exist and Umbraco can write to them
    private readonly string[] _permissionDirs;

    // ensure Umbraco can write to these files (the directories must exist)
    private readonly string[] _permissionFiles = Array.Empty<string>();
    private readonly string _basePath;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FilePermissionHelper" /> class.
    /// </summary>
    public FilePermissionHelper(IOptions<GlobalSettings> globalSettings, IIOHelper ioHelper,
        IHostingEnvironment hostingEnvironment)
    {
        _globalSettings = globalSettings.Value;
        _ioHelper = ioHelper;
        _hostingEnvironment = hostingEnvironment;
        _basePath = hostingEnvironment.MapPathContentRoot("/");
        _permissionDirs = new[]
        {
            hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoCssPath),
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Config),
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Data),
            hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath),
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Preview),
        };
        _packagesPermissionsDirs = new[]
        {
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Bin),
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Umbraco),
            hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoPath),
            hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.Packages),
        };
    }

    /// <inheritdoc />
    public bool RunFilePermissionTestSuite(out Dictionary<FilePermissionTest, IEnumerable<string>> report)
    {
        report = new Dictionary<FilePermissionTest, IEnumerable<string>>();

        EnsureDirectories(_permissionDirs, out IEnumerable<string> errors);
        report[FilePermissionTest.FolderCreation] = errors.ToList();

        EnsureDirectories(_packagesPermissionsDirs, out errors);
        report[FilePermissionTest.FileWritingForPackages] = errors.ToList();

        EnsureFiles(_permissionFiles, out errors);
        report[FilePermissionTest.FileWriting] = errors.ToList();

        EnsureCanCreateSubDirectory(
            _hostingEnvironment.MapPathWebRoot(_globalSettings.UmbracoMediaPhysicalRootPath),
            out errors);
        report[FilePermissionTest.MediaFolderCreation] = errors.ToList();

        return report.Sum(x => x.Value.Count()) == 0;
    }

    private bool EnsureDirectories(string[] dirs, out IEnumerable<string> errors, bool writeCausesRestart = false)
    {
        List<string>? temp = null;
        var success = true;
        foreach (var dir in dirs)
        {
            // we don't want to create/ship unnecessary directories, so
            // here we just ensure we can access the directory, not create it
            var tryAccess = TryAccessDirectory(dir, !writeCausesRestart);
            if (tryAccess)
            {
                continue;
            }

            if (temp == null)
            {
                temp = new List<string>();
            }

            temp.Add(dir.TrimStart(_basePath));
            success = false;
        }

        errors = success ? Enumerable.Empty<string>() : temp ?? Enumerable.Empty<string>();
        return success;
    }

    private bool EnsureFiles(string[] files, out IEnumerable<string> errors)
    {
        List<string>? temp = null;
        var success = true;
        foreach (var file in files)
        {
            var canWrite = TryWriteFile(file);
            if (canWrite)
            {
                continue;
            }

            if (temp == null)
            {
                temp = new List<string>();
            }

            temp.Add(file.TrimStart(_basePath));
            success = false;
        }

        errors = success ? Enumerable.Empty<string>() : temp ?? Enumerable.Empty<string>();
        return success;
    }

    private bool EnsureCanCreateSubDirectory(string dir, out IEnumerable<string> errors)
        => EnsureCanCreateSubDirectories(new[] { dir }, out errors);

    private bool EnsureCanCreateSubDirectories(IEnumerable<string> dirs, out IEnumerable<string> errors)
    {
        List<string>? temp = null;
        var success = true;
        foreach (var dir in dirs)
        {
            var canCreate = TryCreateSubDirectory(dir);
            if (canCreate)
            {
                continue;
            }

            if (temp == null)
            {
                temp = new List<string>();
            }

            temp.Add(dir);
            success = false;
        }

        errors = success ? Enumerable.Empty<string>() : temp ?? Enumerable.Empty<string>();
        return success;
    }

    // tries to create a sub-directory
    // if successful, the sub-directory is deleted
    // creates the directory if needed - does not delete it
    private bool TryCreateSubDirectory(string dir)
    {
        try
        {
            var path = Path.Combine(dir, _ioHelper.CreateRandomFileName());
            Directory.CreateDirectory(path);
            Directory.Delete(path);
            return true;
        }
        catch
        {
            return false;
        }
    }

    // tries to create a file
    // if successful, the file is deleted
    //
    // or
    //
    // use the ACL APIs to avoid creating files
    //
    // if the directory does not exist, do nothing & success
    private bool TryAccessDirectory(string dirPath, bool canWrite)
    {
        try
        {
            if (Directory.Exists(dirPath) == false)
            {
                return true;
            }

            if (canWrite)
            {
                var filePath = dirPath + "/" + _ioHelper.CreateRandomFileName() + ".tmp";
                File.WriteAllText(filePath, "This is an Umbraco internal test file. It is safe to delete it.");
                File.Delete(filePath);
                return true;
            }

            return HasWritePermission(dirPath);
        }
        catch
        {
            return false;
        }
    }

    private bool HasWritePermission(string path)
    {
        var writeAllow = false;
        var writeDeny = false;
        var accessControlList = new DirectorySecurity(
            path,
            AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);

        AuthorizationRuleCollection accessRules;
        try
        {
            accessRules = accessControlList.GetAccessRules(true, true, typeof(SecurityIdentifier));
        }
        catch (Exception)
        {
            // This is not 100% accurate because it could turn out that the current user doesn't
            // have access to read the current permissions but does have write access.
            // I think this is an edge case however
            return false;
        }

        foreach (FileSystemAccessRule rule in accessRules)
        {
            if ((FileSystemRights.Write & rule.FileSystemRights) != FileSystemRights.Write)
            {
                continue;
            }

            if (rule.AccessControlType == AccessControlType.Allow)
            {
                writeAllow = true;
            }
            else if (rule.AccessControlType == AccessControlType.Deny)
            {
                writeDeny = true;
            }
        }

        return writeAllow && writeDeny == false;
    }

    // tries to write into a file
    // fails if the directory does not exist
    private bool TryWriteFile(string file)
    {
        try
        {
            var path = file;
            File.AppendText(path).Close();
            return true;
        }
        catch
        {
            return false;
        }
    }
}
