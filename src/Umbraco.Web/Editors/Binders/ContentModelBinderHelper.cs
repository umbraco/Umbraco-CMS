using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Controllers;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.WebApi;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// Helper methods to bind media/member models
    /// </summary>
    internal class ContentModelBinderHelper
    {
        public TModelSave BindModelFromMultipartRequest<TModelSave>(HttpActionContext actionContext, ModelBindingContext bindingContext)
            where TModelSave : IHaveUploadedFiles
        {
            var result = actionContext.ReadAsMultipart("~/App_Data/TEMP/FileUploads");

            var model = actionContext.GetModelFromMultipartRequest<TModelSave>(result, "contentItem");

            //get the files
            foreach (var file in result.FileData)
            {
                //The name that has been assigned in JS has 2 parts and the second part indicates the property id
                // for which the file belongs.
                var parts = file.Headers.ContentDisposition.Name.Trim('\"').Split('_');
                if (parts.Length != 2)
                {
                    var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = "The request was not formatted correctly the file name's must be underscore delimited";
                    throw new HttpResponseException(response);
                }
                var propAlias = parts[1];

                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');

                model.UploadedFiles.Add(new ContentPropertyFile
                {
                    TempFilePath = file.LocalFileName,
                    PropertyAlias = propAlias,
                    FileName = fileName
                });
            }

            bindingContext.Model = model;

            return model;
        }

        /// <summary>
        /// we will now assign all of the values in the 'save' model to the DTO object
        /// </summary>
        /// <param name="saveModel"></param>
        /// <param name="dto"></param>
        public void MapPropertyValuesFromSaved(IContentProperties<ContentPropertyBasic> saveModel, ContentItemDto dto)
        {
            //NOTE: Don't convert this to linq, this is much quicker
            foreach (var p in saveModel.Properties)
            {
                foreach (var propertyDto in dto.Properties)
                {
                    if (propertyDto.Alias != p.Alias) continue;
                    propertyDto.Value = p.Value;
                    break;
                }
            }
        }
    }
}
