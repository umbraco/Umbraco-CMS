using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// A base class purely used for logging without generics
    /// </summary>
    internal class ContentModelValidator
    {
        protected IUmbracoContextAccessor UmbracoContextAccessor { get; }
        protected ILogger Logger { get; }

        public ContentModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor)
        {
            Logger = logger ?? throw new ArgumentNullException(nameof(logger));
            UmbracoContextAccessor = umbracoContextAccessor ?? throw new ArgumentNullException(nameof(umbracoContextAccessor));
        }
    }

    /// <summary>
    /// A validation helper class used with ContentItemValidationFilterAttribute to be shared between content, media, etc...
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    /// <typeparam name="TModelSave"></typeparam>
    /// <typeparam name="TModelWithProperties"></typeparam>
    /// <remarks>
    /// If any severe errors occur then the response gets set to an error and execution will not continue. Property validation
    /// errors will just be added to the ModelState.
    /// </remarks>
    internal class ContentModelValidator<TPersisted, TModelSave, TModelWithProperties>: ContentModelValidator
        where TPersisted : class, IContentBase
        where TModelSave: IContentSave<TPersisted>
        where TModelWithProperties : IContentProperties<ContentPropertyBasic>
    {
        public ContentModelValidator(ILogger logger, IUmbracoContextAccessor umbracoContextAccessor) : base(logger, umbracoContextAccessor)
        {
        }

        //public void ValidateItem(HttpActionContext actionContext, TModelSave model, IContentProperties<ContentPropertyBasic> modelWithProperties, ContentPropertyCollectionDto dto)
        //{
        //    //now do each validation step
        //    if (ValidateExistingContent(model, actionContext) == false) return;
        //    if (ValidateProperties(model, modelWithProperties, actionContext) == false) return;
        //    if (ValidatePropertyData(model, modelWithProperties, dto, actionContext.ModelState) == false) return;
        //}

        /// <summary>
        /// Ensure the content exists
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public virtual bool ValidateExistingContent(TModelSave postedItem, HttpActionContext actionContext)
        {
            var persistedContent = postedItem.PersistedContent;
            if (persistedContent == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, "content was not found");
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure all of the ids in the post are valid
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modelWithProperties"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        public virtual bool ValidateProperties(TModelSave model, IContentProperties<ContentPropertyBasic> modelWithProperties, HttpActionContext actionContext)
        {
            var persistedContent = model.PersistedContent;
            return ValidateProperties(modelWithProperties.Properties.ToList(), persistedContent.Properties.ToList(), actionContext);
        }

        /// <summary>
        /// This validates that all of the posted properties exist on the persisted entity
        /// </summary>
        /// <param name="postedProperties"></param>
        /// <param name="persistedProperties"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected bool ValidateProperties(List<ContentPropertyBasic> postedProperties, List<Property> persistedProperties, HttpActionContext actionContext)
        {
            foreach (var p in postedProperties)
            {
                if (persistedProperties.Any(property => property.Alias == p.Alias) == false)
                {
                    // TODO: Do we return errors here ? If someone deletes a property whilst their editing then should we just
                    //save the property data that remains? Or inform them they need to reload... not sure. This problem exists currently too i think.

                    var message = $"property with alias: {p.Alias} was not found";
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, new InvalidOperationException(message));
                    return false;
                }

            }
            return true;
        }

        /// <summary>
        /// Validates the data for each property
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modelWithProperties"></param>
        /// <param name="dto"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        /// <remarks>
        /// All property data validation goes into the model state with a prefix of "Properties"
        /// </remarks>
        public virtual bool ValidatePropertiesData(
            TModelSave model,
            TModelWithProperties modelWithProperties,
            ContentPropertyCollectionDto dto,
            ModelStateDictionary modelState)
        {
            var properties = modelWithProperties.Properties.ToDictionary(x => x.Alias, x => x);

            foreach (var p in dto.Properties)
            {
                var editor = p.PropertyEditor;

                if (editor == null)
                {
                    var message = $"Could not find property editor \"{p.DataType.EditorAlias}\" for property with id {p.Id}.";

                    Logger.Warn<ContentModelValidator>(message);
                    continue;
                }

                //get the posted value for this property, this may be null in cases where the property was marked as readonly which means
                //the angular app will not post that value.
                if (!properties.TryGetValue(p.Alias, out var postedProp))
                    continue;

                var postedValue = postedProp.Value;

                ValidatePropertyValue(model, modelWithProperties, editor, p, postedValue, modelState);
    
            }

            return modelState.IsValid;
        }

        /// <summary>
        /// Validates a property's value and adds the error to model state if found
        /// </summary>
        /// <param name="model"></param>
        /// <param name="modelWithProperties"></param>
        /// <param name="editor"></param>
        /// <param name="property"></param>
        /// <param name="postedValue"></param>
        /// <param name="modelState"></param>
        protected virtual void ValidatePropertyValue(
            TModelSave model,
            TModelWithProperties modelWithProperties,
            IDataEditor editor,
            ContentPropertyDto property,
            object postedValue,
            ModelStateDictionary modelState)
        {
            // validate
            var valueEditor = editor.GetValueEditor(property.DataType.Configuration);
            foreach (var r in valueEditor.Validate(postedValue, property.IsRequired, property.ValidationRegExp))
            {
                modelState.AddPropertyError(r, property.Alias, property.Culture);
            }
        }


    }
}
