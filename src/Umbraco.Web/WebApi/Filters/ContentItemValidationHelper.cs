using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding;
using Umbraco.Core;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// A validation helper class used with ContentItemValidationFilterAttribute to be shared between content, media, etc...
    /// </summary>
    /// <typeparam name="TPersisted"></typeparam>
    /// <typeparam name="TModelSave"></typeparam>
    /// <remarks>
    /// If any severe errors occur then the response gets set to an error and execution will not continue. Property validation
    /// errors will just be added to the ModelState.
    /// </remarks>
    internal class ContentItemValidationHelper<TPersisted, TModelSave>
        where TPersisted : class, IContentBase
        where TModelSave : ContentBaseItemSave<TPersisted>
    {
     
        public void ValidateItem(HttpActionContext actionContext, string argumentName)
        {
            var contentItem = actionContext.ActionArguments[argumentName] as TModelSave;
            if (contentItem == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No " + typeof(TModelSave) + " found in request");
                return;
            }

            ValidateItem(actionContext, contentItem);

        }

        public void ValidateItem(HttpActionContext actionContext, TModelSave contentItem)
        {
            //now do each validation step
            if (ValidateExistingContent(contentItem, actionContext) == false) return;
            if (ValidateProperties(contentItem, actionContext) == false) return;
            if (ValidatePropertyData(contentItem, actionContext) == false) return;
        }

        /// <summary>
        /// Ensure the content exists
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool ValidateExistingContent(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
        {
            if (postedItem.PersistedContent == null)
            {
                var message = string.Format("content with id: {0} was not found", postedItem.Id);
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                return false;
            }

            return true;
        }

        /// <summary>
        /// Ensure all of the ids in the post are valid
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected virtual bool ValidateProperties(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
        {
            return ValidateProperties(postedItem.Properties.ToArray(), postedItem.PersistedContent.Properties.ToArray(), actionContext);
        }

        /// <summary>
        /// This validates that all of the posted properties exist on the persisted entity
        /// </summary>
        /// <param name="postedProperties"></param>
        /// <param name="persistedProperties"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        protected bool ValidateProperties(ContentPropertyBasic[] postedProperties , Property[] persistedProperties, HttpActionContext actionContext)
        {
            foreach (var p in postedProperties)
            {
                if (persistedProperties.Any(property => property.Alias == p.Alias) == false)
                {
                    //TODO: Do we return errors here ? If someone deletes a property whilst their editing then should we just
                    //save the property data that remains? Or inform them they need to reload... not sure. This problem exists currently too i think.

                    var message = string.Format("property with alias: {0} was not found", p.Alias);
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, new InvalidOperationException(message));
                    return false;
                }

            }
            return true;
        } 

        /// <summary>
        /// Validates the data for each property
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <returns></returns>
        /// <remarks>
        /// All property data validation goes into the modelstate with a prefix of "Properties"
        /// </remarks>
        protected virtual bool ValidatePropertyData(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
        {
            foreach (var p in postedItem.ContentDto.Properties)
            {
                var editor = p.PropertyEditor;
                if (editor == null)
                {
                    var message = string.Format("The property editor with alias: {0} was not found for property with id {1}", p.DataType.PropertyEditorAlias, p.Id);
                    LogHelper.Warn<ContentItemValidationHelper<TPersisted, TModelSave>>(message);
                    //actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                    //return false;
                    continue;
                }

                //get the posted value for this property
                var postedValue = postedItem.Properties.Single(x => x.Alias == p.Alias).Value;

                //get the pre-values for this property
                var preValues = p.PreValues;

                //TODO: when we figure out how to 'override' certain pre-value properties we'll either need to:
                // * Combine the preValues with the overridden values stored with the document type property (but how to combine?)
                // * Or, pass in the overridden values stored with the doc type property separately

                foreach (var result in editor.ValueEditor.Validators.SelectMany(v => v.Validate(postedValue, preValues, editor)))
                {
                    actionContext.ModelState.AddPropertyError(result, p.Alias);
                }

                //Now we need to validate the property based on the PropertyType validation (i.e. regex and required)
                // NOTE: These will become legacy once we have pre-value overrides.
                if (p.IsRequired)
                {
                    foreach (var result in p.PropertyEditor.ValueEditor.RequiredValidator.Validate(postedValue, "", preValues, editor))
                    {
                        actionContext.ModelState.AddPropertyError(result, p.Alias);
                    }
                }

                if (p.ValidationRegExp.IsNullOrWhiteSpace() == false)
                {

                    //We only want to execute the regex statement if:
                    // * the value is null or empty AND it is required OR
                    // * the value is not null or empty
                    //See: http://issues.umbraco.org/issue/U4-4669

                    var asString = postedValue as string;

                    if (
                        //Value is not null or empty
                        (postedValue != null && asString.IsNullOrWhiteSpace() == false)
                        //It's required
                        || (p.IsRequired))
                    {
                        foreach (var result in p.PropertyEditor.ValueEditor.RegexValidator.Validate(postedValue, p.ValidationRegExp, preValues, editor))
                        {
                            actionContext.ModelState.AddPropertyError(result, p.Alias);
                        }
                    }
                    
                }
            }

            return actionContext.ModelState.IsValid;
        }

        
    }
}