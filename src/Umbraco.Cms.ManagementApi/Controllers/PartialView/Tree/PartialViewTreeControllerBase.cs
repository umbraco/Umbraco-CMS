﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.ManagementApi.Controllers.Tree;
using Umbraco.New.Cms.Web.Common.Routing;

namespace Umbraco.Cms.ManagementApi.Controllers.PartialView.Tree;

[ApiVersion("1.0")]
[ApiController]
[VersionedApiBackOfficeRoute($"{Constants.Web.RoutePath.Tree}/{Constants.UdiEntityType.PartialView}")]
[ApiExplorerSettings(GroupName = "Partial View")]
public class PartialViewTreeControllerBase : FileSystemTreeControllerBase
{
    public PartialViewTreeControllerBase(FileSystems fileSystems)
        => FileSystem = fileSystems.PartialViewsFileSystem ??
                        throw new ArgumentException("Missing partial views file system", nameof(fileSystems));

    protected override IFileSystem FileSystem { get; }

    protected override string FileIcon(string path) => Constants.Icons.PartialView;

    protected override string ItemType(string path) => Constants.UdiEntityType.PartialView;
}
