using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class ElementDraftBranchPresentationFactory : IElementDraftBranchPresentationFactory
{
    /// <inheritdoc />
    public ElementDraftBranchResponseModel Create(IContentDraftBranch branch) => new()
    {
        Id = branch.Key,
        Element = new ReferenceByIdModel(branch.ContentKey),
        Name = branch.Name,
        CreateDate = branch.CreateDate,
        UpdateDate = branch.UpdateDate,
        ScheduledPublishDate = branch.ScheduledPublishDate,
    };

    /// <inheritdoc />
    public IEnumerable<ElementDraftBranchResponseModel> CreateMultiple(IEnumerable<IContentDraftBranch> branches)
        => branches.Select(Create);
}
