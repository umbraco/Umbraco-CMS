using Umbraco.Cms.Core;

namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public class TemplateTreeItemResponseModel : EntityTreeItemResponseModel
{
    public override string Type => Constants.UdiEntityType.Template;
}
