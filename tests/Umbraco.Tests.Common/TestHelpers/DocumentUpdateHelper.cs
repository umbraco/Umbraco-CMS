using Umbraco.Cms.Api.Management.ViewModels;
using Umbraco.Cms.Api.Management.ViewModels.Document;
using Umbraco.Cms.Core.Models.ContentEditing;

namespace Umbraco.Cms.Tests.Common.TestHelpers;

public static class DocumentUpdateHelper
{
    public static UpdateDocumentRequestModel CreateInvariantDocumentUpdateRequestModel(ContentCreateModel createModel)
    {
        var updateRequestModel = new UpdateDocumentRequestModel();

        updateRequestModel.Template = ReferenceByIdModel.ReferenceOrNull(createModel.TemplateKey);
        updateRequestModel.Variants =
        [
            new DocumentVariantRequestModel
            {
                Segment = null,
                Culture = null,
                Name = createModel.Variants.FirstOrDefault(v => v.Culture is null && v.Segment is null)?.Name
                        ?? throw new ArgumentException("Could not find an invariant variant for the model name", nameof(createModel)),
            }
        ];
        updateRequestModel.Values = createModel.Properties.Select(x => new DocumentValueModel
        {
            Alias = x.Alias,
            Value = x.Value,
        });

        return updateRequestModel;
    }

    public static CreateDocumentRequestModel CreateDocumentRequestModel(ContentCreateModel createModel)
    {
        var createDocumentRequestModel = new CreateDocumentRequestModel
        {
            Template = ReferenceByIdModel.ReferenceOrNull(createModel.TemplateKey),
            DocumentType = new ReferenceByIdModel(createModel.ContentTypeKey),
            Parent = ReferenceByIdModel.ReferenceOrNull(createModel.ParentKey),
        };

        createDocumentRequestModel.Variants =
        [
            new DocumentVariantRequestModel
            {
                Segment = null,
                Culture = null,
                Name = createModel.Variants.FirstOrDefault(v => v.Culture is null && v.Segment is null)?.Name
                       ?? throw new ArgumentException("Could not find an invariant variant for the model name", nameof(createModel)),
            }
        ];
        createDocumentRequestModel.Values = createModel.Properties.Select(x => new DocumentValueModel
        {
            Alias = x.Alias,
            Value = x.Value,
        });


        return createDocumentRequestModel;
    }
}
