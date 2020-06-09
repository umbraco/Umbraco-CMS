using System;
using System.IO;
using System.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.Net.Http.Headers;
using Umbraco.Core;
using Umbraco.Core.Models.Editors;
using Umbraco.Extensions;
using Umbraco.Web.Common.Exceptions;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Binders
{
    /// <summary>
    /// Helper methods to bind media/member models
    /// </summary>
    internal class ContentModelBinderHelper
    {
        public TModelSave BindModelFromMultipartRequest<TModelSave>(ActionContext actionContext,
            ModelBindingContext bindingContext)
            where TModelSave : IHaveUploadedFiles
        {
            var result = actionContext.ReadAsMultipart(Constants.SystemDirectories.TempFileUploads);

            var model = actionContext.GetModelFromMultipartRequest<TModelSave>(result, "contentItem");

            //get the files
            foreach (var file in result.FileData)
            {
                //The name that has been assigned in JS has 2 or more parts. The second part indicates the property id
                // for which the file belongs, the remaining parts are just metadata that can be used by the property editor.
                var parts = file.Headers.ContentDisposition.Name.Trim('\"').Split('_');
                if (parts.Length < 2)
                {
                    bindingContext.HttpContext.SetReasonPhrase(
                        "The request was not formatted correctly the file name's must be underscore delimited");
                    throw new HttpResponseException(HttpStatusCode.BadRequest);
                }

                var propAlias = parts[1];

                //if there are 3 parts part 3 is always culture
                string culture = null;
                if (parts.Length > 2)
                {
                    culture = parts[2];
                    //normalize to null if empty
                    if (culture.IsNullOrWhiteSpace())
                    {
                        culture = null;
                    }
                }

                //if there are 4 parts part 4 is always segment
                string segment = null;
                if (parts.Length > 3)
                {
                    segment = parts[3];
                    //normalize to null if empty
                    if (segment.IsNullOrWhiteSpace())
                    {
                        segment = null;
                    }
                }

                // TODO: anything after 4 parts we can put in metadata

                var fileName = file.Headers.ContentDisposition.FileName.Trim('\"');

                model.UploadedFiles.Add(new ContentPropertyFile
                {
                    TempFilePath = file.LocalFileName,
                    PropertyAlias = propAlias,
                    Culture = culture,
                    Segment = segment,
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
        public void MapPropertyValuesFromSaved(IContentProperties<ContentPropertyBasic> saveModel,
            ContentPropertyCollectionDto dto)
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
