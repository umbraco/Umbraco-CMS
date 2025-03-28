using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;

namespace Umbraco.Cms.Api.Management.Factories;

internal sealed class DocumentEditingPresentationFactory : ContentEditingPresentationFactory<DocumentValueModel, DocumentVariantRequestModel>, IDocumentEditingPresentationFactory
{
    private readonly IHubContext<DocumentHub> _hubContext;

    public DocumentEditingPresentationFactory(IHubContext<DocumentHub> hubContext)
    {
        _hubContext = hubContext;
    }

    public ContentCreateModel MapCreateModel(CreateDocumentRequestModel requestModel)
    {
        ContentCreateModel model = MapContentEditingModel<ContentCreateModel>(requestModel);
        model.Key = requestModel.Id;
        model.ContentTypeKey = requestModel.DocumentType.Id;
        model.TemplateKey = requestModel.Template?.Id;
        model.ParentKey = requestModel.Parent?.Id;

        return model;
    }

    public ContentUpdateModel MapUpdateModel(UpdateDocumentRequestModel requestModel)
        => MapUpdateContentModel<ContentUpdateModel>(requestModel);

    public ValidateContentUpdateModel MapValidateUpdateModel(ValidateUpdateDocumentRequestModel requestModel)
    {
        ValidateContentUpdateModel model = MapUpdateContentModel<ValidateUpdateDocumentRequestModel>(requestModel);
        model.Cultures = requestModel.Cultures;

        return model;
    }

    private TUpdateModel MapUpdateContentModel<TUpdateModel>(UpdateDocumentRequestModel requestModel)
        where TUpdateModel : ContentUpdateModel, new()
    {
        TUpdateModel model = MapContentEditingModel<TUpdateModel>(requestModel);
        model.TemplateKey = requestModel.Template?.Id;

        return model;
    }

    public async Task TrackUserVisitAsync(Guid documentId, string userId)
    {
        await _hubContext.Clients.Group(documentId.ToString()).SendAsync("UserVisited", userId);
    }

    public async Task TrackUserNavigationAsync(Guid documentId, string userId)
    {
        await _hubContext.Clients.Group(documentId.ToString()).SendAsync("UserNavigated", userId);
    }
}
