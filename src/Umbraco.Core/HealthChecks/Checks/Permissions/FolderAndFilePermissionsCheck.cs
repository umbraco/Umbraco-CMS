// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text;
using Umbraco.Cms.Core.Install;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Permissions;

/// <summary>
///     Health check for the folder and file permissions.
/// </summary>
[HealthCheck(
    "53DBA282-4A79-4B67-B958-B29EC40FCC23",
    "Folder & File Permissions",
    Description = "Checks that the web server folder and file permissions are set correctly for Umbraco to run.",
    Group = "Permissions")]
public class FolderAndFilePermissionsCheck : HealthCheck
{
    private readonly IFilePermissionHelper _filePermissionHelper;
    private readonly ILocalizedTextService _textService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="FolderAndFilePermissionsCheck" /> class.
    /// </summary>
    public FolderAndFilePermissionsCheck(
        ILocalizedTextService textService,
        IFilePermissionHelper filePermissionHelper)
    {
        _textService = textService;
        _filePermissionHelper = filePermissionHelper;
    }

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override Task<IEnumerable<HealthCheckStatus>> GetStatus()
    {
        _filePermissionHelper.RunFilePermissionTestSuite(
            out Dictionary<FilePermissionTest, IEnumerable<string>> errors);

        return Task.FromResult(errors.Select(x => new HealthCheckStatus(GetMessage(x))
        {
            ResultType = x.Value.Any() ? StatusResultType.Error : StatusResultType.Success,
            ReadMoreLink = GetReadMoreLink(x),
            Description = GetErrorDescription(x),
        }));
    }

    /// <summary>
    ///     Executes the action and returns it's status
    /// </summary>
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action) =>
        throw new InvalidOperationException("FolderAndFilePermissionsCheck has no executable actions");

    private string? GetErrorDescription(KeyValuePair<FilePermissionTest, IEnumerable<string>> status)
    {
        if (!status.Value.Any())
        {
            return null;
        }

        var sb = new StringBuilder("The following failed:");

        sb.AppendLine("<ul>");
        foreach (var error in status.Value)
        {
            sb.Append("<li>" + error + "</li>");
        }

        sb.AppendLine("</ul>");
        return sb.ToString();
    }

    private string GetMessage(KeyValuePair<FilePermissionTest, IEnumerable<string>> status)
        => _textService.Localize("permissions", status.Key);

    private string? GetReadMoreLink(KeyValuePair<FilePermissionTest, IEnumerable<string>> status)
    {
        if (!status.Value.Any())
        {
            return null;
        }

        switch (status.Key)
        {
            case FilePermissionTest.FileWriting:
                return Constants.HealthChecks.DocumentationLinks.FolderAndFilePermissionsCheck.FileWriting;
            case FilePermissionTest.FolderCreation:
                return Constants.HealthChecks.DocumentationLinks.FolderAndFilePermissionsCheck.FolderCreation;
            case FilePermissionTest.FileWritingForPackages:
                return Constants.HealthChecks.DocumentationLinks.FolderAndFilePermissionsCheck.FileWritingForPackages;
            case FilePermissionTest.MediaFolderCreation:
                return Constants.HealthChecks.DocumentationLinks.FolderAndFilePermissionsCheck.MediaFolderCreation;
            default:
                return null;
        }
    }
}
