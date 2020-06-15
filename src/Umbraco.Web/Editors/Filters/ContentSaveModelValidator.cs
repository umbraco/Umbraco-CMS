using System.ComponentModel.DataAnnotations;
using System.Web.Http.ModelBinding;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Services;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Validator for <see cref="ContentItemSave"/>
    /// </summary>
    internal class ContentSaveModelValidator : ContentModelValidator<IContent, ContentItemSave, ContentVariantSave>
    {
        public ContentSaveModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor, ILocalizedTextService textService) : base(logger, umbracoContextAccessor, textService)
        {
        }

        protected override void AddPropertyError(ContentItemSave model, ContentVariantSave modelWithProperties, IDataEditor editor, ContentPropertyDto property, ValidationResult validationResult, ModelStateDictionary modelState)
        {
            // Original idea: See if we can build up the JSON + JSON Path
            // SD: I'm just keeping these notes here for later just to remind myself that we might want to take into account the tab number in validation
            // which we might be able to get in the PropertyValidationService anyways?

            // Create a JSON + JSON Path key, see https://gist.github.com/Shazwazza/ad9fcbdb0fdacff1179a9eed88393aa6

            //var json = new PropertyError
            //{
            //    Culture = property.Culture,
            //    Segment = property.Segment
            //};

            // TODO: Hrm, we can't get the tab index without a reference to the content type itself! the IContent doesn't contain a reference to groups/indexes
            // BUT! I think it contains a reference to the group alias so we could use JSON Path for a group alias instead of index like:
            // .tabs[?(@.alias=='Content')]
            //var tabIndex = ??

            //var jsonPath = "$.variants[0].tabs[0].properties[?(@.alias=='title')].value[0]";

            base.AddPropertyError(model, modelWithProperties, editor, property, validationResult, modelState);
        }

    }
}
