using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Api.Management.ViewModels.Document.Collection;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Api.Management.Factories;

public interface IDocumentCollectionPresentationFactory : IContentCollectionPresentationFactory<IContent, DocumentCollectionResponseModel, DocumentValueResponseModel, DocumentVariantResponseModel>
{
}
