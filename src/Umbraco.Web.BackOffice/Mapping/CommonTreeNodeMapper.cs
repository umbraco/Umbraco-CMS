using Microsoft.AspNetCore.Routing;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Web.BackOffice.Trees;
using Umbraco.Cms.Web.Common.Controllers;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.Mapping;

public class CommonTreeNodeMapper
{
    private readonly LinkGenerator _linkGenerator;


    public CommonTreeNodeMapper(LinkGenerator linkGenerator) => _linkGenerator = linkGenerator;


    public string? GetTreeNodeUrl<TController>(IContentBase source)
        where TController : UmbracoApiController, ITreeNodeController =>
        _linkGenerator.GetUmbracoApiService<TController>(controller =>
            controller.GetTreeNode(source.Key.ToString("N"), null));
}
