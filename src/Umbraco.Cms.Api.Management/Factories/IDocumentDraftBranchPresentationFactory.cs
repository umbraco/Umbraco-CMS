using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for named document draft branches.
/// </summary>
public interface IDocumentDraftBranchPresentationFactory
{
    /// <summary>
    /// Creates a <see cref="DocumentDraftBranchResponseModel"/> from the given <see cref="IContentDraftBranch"/>.
    /// </summary>
    /// <param name="branch">The draft branch to create the response model from.</param>
    /// <returns>The created <see cref="DocumentDraftBranchResponseModel"/>.</returns>
    DocumentDraftBranchResponseModel Create(IContentDraftBranch branch);

    /// <summary>
    /// Creates multiple document draft branch presentation models from the provided draft branches.
    /// </summary>
    /// <param name="branches">The draft branches to create presentation models for.</param>
    /// <returns>The created <see cref="DocumentDraftBranchResponseModel"/> instances.</returns>
    IEnumerable<DocumentDraftBranchResponseModel> CreateMultiple(IEnumerable<IContentDraftBranch> branches);
}
