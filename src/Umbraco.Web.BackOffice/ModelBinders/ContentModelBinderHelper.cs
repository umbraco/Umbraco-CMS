using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Hosting;
using Umbraco.Cms.Core.Models.ContentEditing;
using Umbraco.Cms.Core.Models.Editors;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Extensions;

namespace Umbraco.Cms.Web.BackOffice.ModelBinders;

/// <summary>
///     Helper methods to bind media/member models
/// </summary>
internal class ContentModelBinderHelper
{
    public async Task<T?> BindModelFromMultipartRequestAsync<T>(
        IJsonSerializer jsonSerializer,
        IHostingEnvironment hostingEnvironment,
        ModelBindingContext bindingContext)
        where T : class, IHaveUploadedFiles
    {
        var modelName = bindingContext.ModelName;

        ValueProviderResult valueProviderResult = bindingContext.ValueProvider.GetValue(modelName);

        if (valueProviderResult == ValueProviderResult.None)
        {
            return null;
        }

        bindingContext.ModelState.SetModelValue(modelName, valueProviderResult);

        var value = valueProviderResult.FirstValue;

        // Check if the argument value is null or empty
        if (string.IsNullOrEmpty(value))
        {
            return null;
        }

        T? model = jsonSerializer.Deserialize<T>(value);
        if (model is null)
        {
            // Non-integer arguments result in model state errors
            bindingContext.ModelState.TryAddModelError(
                modelName, $"Cannot deserialize {modelName} as {nameof(T)}.");

            return null;
        }

        //Handle file uploads
        foreach (IFormFile formFile in bindingContext.HttpContext.Request.Form.Files)
        {
            //The name that has been assigned in JS has 2 or more parts. The second part indicates the property id
            // for which the file belongs, the remaining parts are just metadata that can be used by the property editor.
            var parts = formFile.Name.Trim(Constants.CharArrays.DoubleQuote).Split(Constants.CharArrays.Underscore);
            if (parts.Length < 2)
            {
                bindingContext.HttpContext.SetReasonPhrase(
                    "The request was not formatted correctly the file name's must be underscore delimited");
                return null;
            }

            var propAlias = parts[1];

            //if there are 3 parts part 3 is always culture
            string? culture = null;
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
            string? segment = null;
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

            var fileName = formFile.FileName.Trim(Constants.CharArrays.DoubleQuote);

            var tempFileUploadFolder =
                hostingEnvironment.MapPathContentRoot(Constants.SystemDirectories.TempFileUploads);
            Directory.CreateDirectory(tempFileUploadFolder);
            var tempFilePath = Path.Combine(tempFileUploadFolder, Guid.NewGuid().ToString());

            using (FileStream stream = File.Create(tempFilePath))
            {
                await formFile.CopyToAsync(stream);
            }

            model.UploadedFiles.Add(new ContentPropertyFile
            {
                TempFilePath = tempFilePath,
                PropertyAlias = propAlias,
                Culture = culture,
                Segment = segment,
                FileName = fileName
            });
        }

        return model;
    }

    /// <summary>
    ///     we will now assign all of the values in the 'save' model to the DTO object
    /// </summary>
    /// <param name="saveModel"></param>
    /// <param name="dto"></param>
    public void MapPropertyValuesFromSaved(IContentProperties<ContentPropertyBasic> saveModel,
        ContentPropertyCollectionDto? dto)
    {
        //NOTE: Don't convert this to linq, this is much quicker
        foreach (ContentPropertyBasic p in saveModel.Properties)
        {
            if (dto is not null)
            {
                foreach (ContentPropertyDto propertyDto in dto.Properties)
                {
                    if (propertyDto.Alias != p.Alias)
                    {
                        continue;
                    }

                    propertyDto.Value = p.Value;
                    break;
                }
            }
        }
    }
}
