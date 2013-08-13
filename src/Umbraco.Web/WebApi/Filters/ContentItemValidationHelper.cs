using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
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
    /// <remarks>
    /// If any severe errors occur then the response gets set to an error and execution will not continue. Property validation
    /// errors will just be added to the ModelState.
    /// </remarks>
    internal class ContentItemValidationHelper<TPersisted>
        where TPersisted : IContentBase
    {
        private readonly ApplicationContext _applicationContext;

        public ContentItemValidationHelper(ApplicationContext applicationContext)
        {
            _applicationContext = applicationContext;
        }

        public ContentItemValidationHelper()
            : this(ApplicationContext.Current)
        {
            
        }

        public void ValidateItem(HttpActionContext actionContext, string argumentName)
        {
            var contentItem = actionContext.ActionArguments[argumentName] as ContentItemSave<TPersisted>;
            if (contentItem == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No " + typeof(ContentItemSave<IContent>) + " found in request");
                return;
            }

            ValidateItem(actionContext, contentItem);

        }

        public void ValidateItem(HttpActionContext actionContext, ContentItemSave<TPersisted> contentItem)
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
        private bool ValidateExistingContent(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
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
        private bool ValidateProperties(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
        {
            //ensure the property actually exists in our server side properties
            //var propertyAliases = postedItem.ContentDto.Properties.Select(x => x.Alias).ToArray();

            foreach (var p in postedItem.Properties)
            {
                //if (propertyAliases.Contains(p.Alias) == false)
                if (postedItem.PersistedContent.Properties[p.Alias] == null)
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
        private bool ValidatePropertyData(ContentItemBasic<ContentPropertyBasic, TPersisted> postedItem, HttpActionContext actionContext)
        {
            foreach (var p in postedItem.ContentDto.Properties)
            {
                var editor = p.PropertyEditor;
                if (editor == null)
                {
                    var message = string.Format("The property editor with id: {0} was not found for property with id {1}", p.DataType.ControlId, p.Id);
                    LogHelper.Warn<ContentItemValidationHelper<TPersisted>>(message);
                    //actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                    //return false;
                    continue;
                }

                //get the posted value for this property
                var postedValue = postedItem.Properties.Single(x => x.Alias == p.Alias).Value;

                //get the pre-values for this property
                var preValues = _applicationContext.Services.DataTypeService.GetPreValueAsString(p.DataType.Id);

                //TODO: when we figure out how to 'override' certain pre-value properties we'll either need to:
                // * Combine the preValues with the overridden values stored with the document type property (but how to combine?)
                // * Or, pass in the overridden values stored with the doc type property separately

                foreach (var v in editor.ValueEditor.Validators)
                {
                    foreach (var result in v.Validate(postedValue, preValues, editor))
                    {
                        //if there are no member names supplied then we assume that the validation message is for the overall property
                        // not a sub field on the property editor
                        if (!result.MemberNames.Any())
                        {
                            //add a model state error for the entire property
                            actionContext.ModelState.AddModelError(string.Format("{0}.{1}", "Properties", p.Alias), result.ErrorMessage);
                        }
                        else
                        {
                            //there's assigned field names so we'll combine the field name with the property name
                            // so that we can try to match it up to a real sub field of this editor
                            foreach (var field in result.MemberNames)
                            {
                                actionContext.ModelState.AddModelError(string.Format("{0}.{1}.{2}", "Properties", p.Alias, field), result.ErrorMessage);
                            }
                        }
                    }
                }

                //Now we need to validate the property based on the PropertyType validation (i.e. regex and required)
                // NOTE: These will become legacy once we have pre-value overrides.
                if (p.IsRequired)
                {
                    foreach (var result in p.PropertyEditor.ValueEditor.RequiredValidator.Validate(postedValue, "", preValues, editor))
                    {
                        //add a model state error for the entire property
                        actionContext.ModelState.AddModelError(string.Format("{0}.{1}", "Properties", p.Alias), result.ErrorMessage);
                    }
                }

                if (!p.ValidationRegExp.IsNullOrWhiteSpace())
                {
                    foreach (var result in p.PropertyEditor.ValueEditor.RegexValidator.Validate(postedValue, p.ValidationRegExp, preValues, editor))
                    {
                        //add a model state error for the entire property
                        actionContext.ModelState.AddModelError(string.Format("{0}.{1}", "Properties", p.Alias), result.ErrorMessage);
                    }
                }
            }

            return actionContext.ModelState.IsValid;
        }
    }
}