using Umbraco.Cms.Api.Management.ViewModels.Element;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

/// <summary>
/// Represents a factory responsible for creating presentation models for named element draft branches.
/// </summary>
public interface IElementDraftBranchPresentationFactory
{
    /// <summary>
    /// Creates an <see cref="ElementDraftBranchResponseModel"/> from the given <see cref="IContentDraftBranch"/>.
    /// </summary>
    /// <param name="branch">The draft branch to create the response model from.</param>
    /// <returns>The created <see cref="ElementDraftBranchResponseModel"/>.</returns>
    ElementDraftBranchResponseModel Create(IContentDraftBranch branch);

    /// <summary>
    /// Creates multiple element draft branch presentation models from the provided draft branches.
    /// </summary>
    /// <param name="branches">The draft branches to create presentation models for.</param>
    /// <returns>The created <see cref="ElementDraftBranchResponseModel"/> instances.</returns>
    IEnumerable<ElementDraftBranchResponseModel> CreateMultiple(IEnumerable<IContentDraftBranch> branches);
}
