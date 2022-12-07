﻿using Microsoft.AspNetCore.Mvc;
using Umbraco.Cms.Api.Management.Routing;

namespace Umbraco.Cms.Api.Management.Controllers.Searcher;

[ApiController]
[VersionedApiBackOfficeRoute("searcher")]
[ApiExplorerSettings(GroupName = "Searcher")]
public class SearcherControllerBase : ManagementApiControllerBase
{
}
