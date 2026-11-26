using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <inheritdoc />
public class DocumentDraftBranchPresentationFactory : IDocumentDraftBranchPresentationFactory
{
    /// <inheritdoc />
    public DocumentDraftBranchResponseModel Create(IContentDraftBranch branch) => new()
    {
        Id = branch.Key,
        Document = new ReferenceByIdModel(branch.ContentKey),
        Name = branch.Name,
        CreateDate = branch.CreateDate,
        UpdateDate = branch.UpdateDate,
        ScheduledPublishDate = branch.ScheduledPublishDate,
    };

    /// <inheritdoc />
    public IEnumerable<DocumentDraftBranchResponseModel> CreateMultiple(IEnumerable<IContentDraftBranch> branches)
        => branches.Select(Create);
}
