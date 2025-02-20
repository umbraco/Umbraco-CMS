namespace Umbraco.Cms.Api.Management.ViewModels.Tree;

public abstract class ContentTreeItemResponseModel : EntityTreeItemResponseModel
{
    public bool NoAccess { get; set; }

    public bool IsTrashed { get; set; }

    public Guid Id { get; set; }

    public DateTimeOffset CreateDate { get; set; }
}
