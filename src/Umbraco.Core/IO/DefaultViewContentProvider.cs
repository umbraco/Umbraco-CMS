using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.IO;

public class DefaultViewContentProvider : IDefaultViewContentProvider
{
    public string GetDefaultFileContent(string? layoutPageAlias = null, string? modelClassName = null, string? modelNamespace = null, string? modelNamespaceAlias = null)
    {
        var content = new StringBuilder();

        if (string.IsNullOrWhiteSpace(modelNamespaceAlias))
        {
            modelNamespaceAlias = "ContentModels";
        }

        // either
        // @inherits Umbraco.Web.Mvc.UmbracoViewPage
        // @inherits Umbraco.Web.Mvc.UmbracoViewPage<ModelClass>
        content.AppendLine("@using Umbraco.Cms.Web.Common.PublishedModels;");
        content.Append("@inherits Umbraco.Cms.Web.Common.Views.UmbracoViewPage");
        if (modelClassName.IsNullOrWhiteSpace() == false)
        {
            content.Append("<");
            if (modelNamespace.IsNullOrWhiteSpace() == false)
            {
                content.Append(modelNamespaceAlias);
                content.Append(".");
            }

            content.Append(modelClassName);
            content.Append(">");
        }

        content.Append("\r\n");

        // if required, add
        // @using ContentModels = ModelNamespace;
        if (modelClassName.IsNullOrWhiteSpace() == false && modelNamespace.IsNullOrWhiteSpace() == false)
        {
            content.Append("@using ");
            content.Append(modelNamespaceAlias);
            content.Append(" = ");
            content.Append(modelNamespace);
            content.Append(";\r\n");
        }

        // either
        // Layout = null;
        // Layout = "layoutPage.cshtml";
        content.Append("@{\r\n\tLayout = ");
        if (layoutPageAlias.IsNullOrWhiteSpace())
        {
            content.Append("null");
        }
        else
        {
            content.Append("\"");
            content.Append(layoutPageAlias);
            content.Append(".cshtml\"");
        }

        content.Append(";\r\n}");
        return content.ToString();
    }
}
