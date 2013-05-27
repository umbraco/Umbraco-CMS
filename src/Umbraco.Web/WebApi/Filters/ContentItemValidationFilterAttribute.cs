using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http.Controllers;
using System.Web.Http.Filters;
using Umbraco.Core;
using Umbraco.Core.PropertyEditors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.Models.Mapping;

namespace Umbraco.Web.WebApi.Filters
{
    /// <summary>
    /// Validates the content item
    /// </summary>
    /// <remarks>
    /// There's various validation happening here both value validation and structure validation
    /// to ensure that malicious folks are not trying to post invalid values or to invalid properties.
    /// </remarks>
    internal class ContentItemValidationFilterAttribute : ActionFilterAttribute
    {
        private readonly ApplicationContext _applicationContext;
        private readonly ContentModelMapper _contentMapper;

        public ContentItemValidationFilterAttribute(ApplicationContext applicationContext, ContentModelMapper contentMapper)
        {
            _applicationContext = applicationContext;
            _contentMapper = contentMapper;
        }

        public ContentItemValidationFilterAttribute()
            : this(ApplicationContext.Current, new ContentModelMapper(ApplicationContext.Current))
        {
            
        }

        /// <summary>
        /// Returns true so that other filters can execute along with this one
        /// </summary>
        public override bool AllowMultiple
        {
            get { return true; }
        }

        /// <summary>
        /// Performs the validation
        /// </summary>
        /// <param name="actionContext"></param>
        public override void OnActionExecuting(HttpActionContext actionContext)
        {
            var contentItem = actionContext.ActionArguments["contentItem"] as ContentItemSave;
            if (contentItem == null)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.BadRequest, "No " + typeof(ContentItemSave) + " found in request");
                return;
            }

            //now do each validation step
            ContentItemDto existingContent;
            if (!ValidateExistingContent(contentItem, actionContext, out existingContent)) return;
            if (!ValidateProperties(contentItem, existingContent, actionContext)) return;
            if (!ValidateData(contentItem, existingContent, actionContext)) return;
        }

        /// <summary>
        /// Ensure the content exists
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <param name="found"></param>
        /// <returns></returns>
        private bool ValidateExistingContent(ContentItemSave postedItem, HttpActionContext actionContext, out ContentItemDto found)
        {
            var item = _applicationContext.Services.ContentService.GetById(postedItem.Id);
            
            if (item == null)
            {
                var message = string.Format("content with id: {0} was not found", postedItem.Id);
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                found = null;
                return false;
            }

            found = _contentMapper.ToContentItemDto(item);
            return true;
        }

        /// <summary>
        /// Ensure all of the ids in the post are valid
        /// </summary>
        /// <param name="postedItem"></param>
        /// <param name="actionContext"></param>
        /// <param name="realItem"></param>
        /// <returns></returns>
        private bool ValidateProperties(ContentItemSave postedItem, ContentItemDto realItem, HttpActionContext actionContext)
        {
            foreach (var p in postedItem.Properties)
            {
                //ensure the property actually exists in our server side properties
                if (!realItem.Properties.Contains(p))
                {
                    //TODO: Do we return errors here ? If someone deletes a property whilst their editing then should we just
                    //save the property data that remains? Or inform them they need to reload... not sure. This problem exists currently too i think.

                    var message = string.Format("property with id: {0} was not found", p.Id);
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                    return false;
                }
            }
            return true;
        }

        //TODO: Validate the property type data
        private bool ValidateData(ContentItemSave postedItem, ContentItemDto realItem, HttpActionContext actionContext)
        {
            foreach (var p in realItem.Properties)
            {
                var editor = PropertyEditorResolver.Current.GetById(p.DataType.ControlId);
                if (editor == null)
                {
                    var message = string.Format("The property editor with id: {0} was not found for property with id {1}", p.DataType.ControlId, p.Id);
                    actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.NotFound, message);
                    return false;
                }

                //get the posted value for this property
                var postedValue = postedItem.Properties.Single(x => x.Id == p.Id).Value;

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
                            actionContext.ModelState.AddModelError(p.Alias, result.ErrorMessage);
                        }
                        else
                        {
                            //there's assigned field names so we'll combine the field name with the property name
                            // so that we can try to match it up to a real sub field of this editor
                            foreach (var field in result.MemberNames)
                            {
                                actionContext.ModelState.AddModelError(string.Format("{0}.{1}", p.Alias, field), result.ErrorMessage);
                            }
                        }
                    }
                }
            }

            //create the response if there any errors
            if (!actionContext.ModelState.IsValid)
            {
                actionContext.Response = actionContext.Request.CreateErrorResponse(HttpStatusCode.Forbidden, actionContext.ModelState);
            }

            return actionContext.ModelState.IsValid;
        }

    }
}