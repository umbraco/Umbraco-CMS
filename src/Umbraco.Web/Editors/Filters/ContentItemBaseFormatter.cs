using System;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using System.Web.Http;
using Newtonsoft.Json;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.Editors.Filters
{
    /// <summary>
    /// Binds the content model to the controller action for the posted multi-part Post
    /// </summary>
    internal abstract class ContentItemBaseFormatter<TPersisted, TModelSave> : MediaTypeFormatter
        where TPersisted : class, IContentBase
        where TModelSave : ContentBaseItemSave<TPersisted>
    {
        protected ApplicationContext ApplicationContext { get; private set; }

        public override bool CanReadType(Type type)
        {
            return (type == typeof(TModelSave));
        }

        public override bool CanWriteType(Type type)
        {
            return false;
        }

        /// <summary>
        /// Constructor
        /// </summary>
        /// <param name="applicationContext"></param>
        internal ContentItemBaseFormatter(ApplicationContext applicationContext)
        {
            ApplicationContext = applicationContext;

            this.SupportedMediaTypes.Clear();
            this.SupportedMediaTypes.Add(new MediaTypeHeaderValue("multipart/form-data"));
        }

        /// <summary>
        /// Asynchronously deserializes an object of the specified type.
        /// </summary>
        /// <returns>
        /// A <see cref="T:System.Threading.Tasks.Task"/> whose result will be the object instance that has been read.
        /// </returns>
        /// <param name="type">The type of object to deserialize.</param><param name="readStream">The <see cref="T:System.IO.Stream"/> to read.</param><param name="content">The <see cref="T:System.Net.Http.HttpContent"/> for the content being read.</param><param name="formatterLogger">The <see cref="T:System.Net.Http.Formatting.IFormatterLogger"/> to log events to.</param>
        public override async Task<object> ReadFromStreamAsync(Type type, Stream readStream, HttpContent content, IFormatterLogger formatterLogger)
        {
            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var result = await content.ReadAsMultipartAsync(provider);

            if (result.FormData["contentItem"] == null)
            {
                const string errMsg = "The request was not formatted correctly and is missing the 'contentItem' parameter";
                formatterLogger.LogError(string.Empty, errMsg);
                var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                {
                    ReasonPhrase = errMsg
                };
                throw new HttpResponseException(response);
            }

            //get the string json from the request
            var contentItem = result.FormData["contentItem"];

            //deserialize into our model
            var model = JsonConvert.DeserializeObject<TModelSave>(contentItem);

            //get the files
            foreach (var file in result.FileData)
            {
                //The name that has been assigned in JS has 2 parts and the second part indicates the property id 
                // for which the file belongs.
                var parts = file.Headers.ContentDisposition.Name.Trim(new char[] { '\"' }).Split('_');
                if (parts.Length != 2)
                {
                    formatterLogger.LogError(string.Empty, "The request was not formatted correctly the file name's must be underscore delimited");
                    var response = new HttpResponseMessage(HttpStatusCode.BadRequest)
                    {
                        ReasonPhrase = "The request was not formatted correctly the file name's must be underscore delimited"
                    };
                    throw new HttpResponseException(response);
                }
                var propAlias = parts[1];

                var fileName = file.Headers.ContentDisposition.FileName.Trim(new char[] { '\"' });

                model.UploadedFiles.Add(new ContentItemFile
                {
                    TempFilePath = file.LocalFileName,
                    PropertyAlias = propAlias,
                    FileName = fileName
                });
            }

            model.PersistedContent = ContentControllerBase.IsCreatingAction(model.Action) ? CreateNew(model) : GetExisting(model);

            //create the dto from the persisted model
            if (model.PersistedContent != null)
            {
                model.ContentDto = MapFromPersisted(model);
            }
            if (model.ContentDto != null)
            {
                //now map all of the saved values to the dto
                MapPropertyValuesFromSaved(model, model.ContentDto);
            }

            return model;
        }

        /// <summary>
        /// we will now assign all of the values in the 'save' model to the DTO object
        /// </summary>
        /// <param name="saveModel"></param>
        /// <param name="dto"></param>
        private static void MapPropertyValuesFromSaved(TModelSave saveModel, ContentItemDto<TPersisted> dto)
        {
            foreach (var p in saveModel.Properties.Where(p => dto.Properties.Any(x => x.Alias == p.Alias)))
            {
                dto.Properties.Single(x => x.Alias == p.Alias).Value = p.Value;
            }
        }

        protected abstract TPersisted GetExisting(TModelSave model);
        protected abstract TPersisted CreateNew(TModelSave model);
        protected abstract ContentItemDto<TPersisted> MapFromPersisted(TModelSave model);

    }
}