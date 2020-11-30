using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Security.AccessControl;
using Umbraco.Core;
using Umbraco.Core.Install;
using Umbraco.Core.IO;
using Umbraco.Web.PublishedCache;
using Umbraco.Core.Configuration.Models;
using Microsoft.Extensions.Options;
using Umbraco.Core.Hosting;

namespace Umbraco.Web.Install
{
    public class FilePermissionHelper : IFilePermissionHelper
    {
        // ensure that these directories exist and Umbraco can write to them
        private readonly string[] _permissionDirs;
        private readonly string[] _packagesPermissionsDirs;

        // ensure Umbraco can write to these files (the directories must exist)
        private readonly string[] _permissionFiles = { };
        private readonly GlobalSettings _globalSettings;
        private readonly IIOHelper _ioHelper;
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly IPublishedSnapshotService _publishedSnapshotService;

        public FilePermissionHelper(IOptions<GlobalSettings> globalSettings, IIOHelper ioHelper, IHostingEnvironment hostingEnvironment, IPublishedSnapshotService publishedSnapshotService)
        {
            _globalSettings = globalSettings.Value;
            _ioHelper = ioHelper;
            _hostingEnvironment = hostingEnvironment;
            _publishedSnapshotService = publishedSnapshotService;
            _permissionDirs = new[] { _globalSettings.UmbracoCssPath, Constants.SystemDirectories.Config, Constants.SystemDirectories.Data, _globalSettings.UmbracoMediaPath, Constants.SystemDirectories.Preview };
            _packagesPermissionsDirs = new[] { Constants.SystemDirectories.Bin, _globalSettings.UmbracoPath, Constants.SystemDirectories.Packages };
        }

        public bool RunFilePermissionTestSuite(out Dictionary<string, IEnumerable<string>> report)
        {
            report = new Dictionary<string, IEnumerable<string>>();

            if (EnsureDirectories(_permissionDirs, out var errors) == false)
                report["Folder creation failed"] = errors.ToList();

            if (EnsureDirectories(_packagesPermissionsDirs, out errors) == false)
                report["File writing for packages failed"] = errors.ToList();

            if (EnsureFiles(_permissionFiles, out errors) == false)
                report["File writing failed"] = errors.ToList();

            if (TestPublishedSnapshotService(out errors) == false)
                report["Published snapshot environment check failed"] = errors.ToList();

            if (EnsureCanCreateSubDirectory(_globalSettings.UmbracoMediaPath, out errors) == false)
                report["Media folder creation failed"] = errors.ToList();

            return report.Count == 0;
        }

        /// <summary>
        /// This will test the directories for write access
        /// </summary>
        /// <param name="dirs"></param>
        /// <param name="errors"></param>
        /// <param name="writeCausesRestart">
        /// If this is false, the easiest way to test for write access is to write a temp file, however some folder will cause
        /// an App Domain restart if a file is written to the folder, so in that case we need to use the ACL APIs which aren't as
        /// reliable but we cannot write a file since it will cause an app domain restart.
        /// </param>
        /// <returns></returns>
        public bool EnsureDirectories(string[] dirs, out IEnumerable<string> errors, bool writeCausesRestart = false)
        {
            List<string> temp = null;
            var success = true;
            foreach (var dir in dirs)
            {
                // we don't want to create/ship unnecessary directories, so
                // here we just ensure we can access the directory, not create it
                var tryAccess = TryAccessDirectory(dir, !writeCausesRestart);
                if (tryAccess) continue;

                if (temp == null) temp = new List<string>();
                temp.Add(dir);
                success = false;
            }

            errors = success ? Enumerable.Empty<string>() : temp;
            return success;
        }

        public bool EnsureFiles(string[] files, out IEnumerable<string> errors)
        {
            List<string> temp = null;
            var success = true;
            foreach (var file in files)
            {
                var canWrite = TryWriteFile(file);
                if (canWrite) continue;

                if (temp == null) temp = new List<string>();
                temp.Add(file);
                success = false;
            }

            errors = success ? Enumerable.Empty<string>() : temp;
            return success;
        }

        public bool EnsureCanCreateSubDirectory(string dir, out IEnumerable<string> errors)
        {
            return EnsureCanCreateSubDirectories(new[] { dir }, out errors);
        }

        public bool EnsureCanCreateSubDirectories(IEnumerable<string> dirs, out IEnumerable<string> errors)
        {
            List<string> temp = null;
            var success = true;
            foreach (var dir in dirs)
            {
                var canCreate = TryCreateSubDirectory(dir);
                if (canCreate) continue;

                if (temp == null) temp = new List<string>();
                temp.Add(dir);
                success = false;
            }

            errors = success ? Enumerable.Empty<string>() : temp;
            return success;
        }

        public bool TestPublishedSnapshotService(out IEnumerable<string> errors)
        {
            return _publishedSnapshotService.EnsureEnvironment(out errors);
        }

        // tries to create a sub-directory
        // if successful, the sub-directory is deleted
        // creates the directory if needed - does not delete it
        private bool TryCreateSubDirectory(string dir)
        {
            try
            {
                var path = _hostingEnvironment.MapPathContentRoot(dir + "/" + _ioHelper.CreateRandomFileName());
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
        public bool TryAccessDirectory(string dir, bool canWrite)
        {
            try
            {
                var dirPath = _hostingEnvironment.MapPathContentRoot(dir);

                if (Directory.Exists(dirPath) == false)
                    return true;

                if (canWrite)
                {
                    var filePath = dirPath + "/" + _ioHelper.CreateRandomFileName() + ".tmp";
                    File.WriteAllText(filePath, "This is an Umbraco internal test file. It is safe to delete it.");
                    File.Delete(filePath);
                    return true;
                }
                else
                {
                    return HasWritePermission(dirPath);
                }
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
            var accessControlList = new DirectorySecurity(path, AccessControlSections.Access | AccessControlSections.Owner | AccessControlSections.Group);
            if (accessControlList == null)
                return false;
            AuthorizationRuleCollection accessRules;
            try
            {
                accessRules = accessControlList.GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
                if (accessRules == null)
                    return false;
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
                    continue;

                if (rule.AccessControlType == AccessControlType.Allow)
                    writeAllow = true;
                else if (rule.AccessControlType == AccessControlType.Deny)
                    writeDeny = true;
            }

            return writeAllow && writeDeny == false;
        }

        // tries to write into a file
        // fails if the directory does not exist
        private bool TryWriteFile(string file)
        {
            try
            {
                var path = _hostingEnvironment.MapPathContentRoot(file);
                File.AppendText(path).Close();
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
