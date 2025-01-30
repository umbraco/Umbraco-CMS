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
                Name = createModel.InvariantName!,
            }
        ];
        updateRequestModel.Values = createModel.InvariantProperties.Select(x => new DocumentValueModel
        {
            Alias = x.Alias,
            Value = x.Value,
        });

        return updateRequestModel;
    }
}
