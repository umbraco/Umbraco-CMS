using System;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Net.Http.Formatting;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;
using System.Web.Http.Controllers;
using System.Web.Http.ModelBinding.Binders;
using System.Web.Http.Validation;
using System.Web.ModelBinding;
using System.Web.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Umbraco.Core;
using Umbraco.Core.IO;
using Umbraco.Core.Models;
using Umbraco.Core.Models.Editors;
using Umbraco.Core.Services;
using Umbraco.Web.Composing;
using Umbraco.Web.Editors;
using Umbraco.Web.Models.ContentEditing;
using Umbraco.Web.PublishedCache;
using Umbraco.Web.Routing;
using Umbraco.Web.Security;
using Umbraco.Web.WebApi.Filters;
using IModelBinder = System.Web.Http.ModelBinding.IModelBinder;
using ModelBindingContext = System.Web.Http.ModelBinding.ModelBindingContext;
using ModelMetadata = System.Web.Http.Metadata.ModelMetadata;
using ModelMetadataProvider = System.Web.Http.Metadata.ModelMetadataProvider;
using MutableObjectModelBinder = System.Web.Http.ModelBinding.Binders.MutableObjectModelBinder;
using Task = System.Threading.Tasks.Task;

namespace Umbraco.Web.Editors.Binders
{
    /// <inheritdoc />
    /// <summary>
    /// Binds the content model to the controller action for the posted multi-part Post
    /// </summary>
    internal abstract class ContentItemBaseBinder<TPersisted, TModelSave> : IModelBinder
    where TPersisted : class, IContentBase
        //where TModelSave : ContentBaseItemSave<TPersisted>
    {
        protected Core.Logging.ILogger Logger { get; }
        protected ServiceContext Services { get; }
        protected IUmbracoContextAccessor UmbracoContextAccessor { get; }

        protected ContentItemBaseBinder() : this(Current.Logger, Current.Services, Current.UmbracoContextAccessor)
        {
        }

        protected ContentItemBaseBinder(Core.Logging.ILogger logger, ServiceContext services, IUmbracoContextAccessor umbracoContextAccessor)
        {
            Logger = logger;
            Services = services;
            UmbracoContextAccessor = umbracoContextAccessor;
        }

        public virtual bool BindModel(HttpActionContext actionContext, ModelBindingContext bindingContext)
        {
            if (actionContext.Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var result = GetMultiPartResult(actionContext);

            var model = GetModel(actionContext, bindingContext, result);
            bindingContext.Model = model;

            return bindingContext.Model != null;
        }

        private MultipartFormDataStreamProvider GetMultiPartResult(HttpActionContext actionContext)
        {
            var root = IOHelper.MapPath("~/App_Data/TEMP/FileUploads");
            //ensure it exists
            Directory.CreateDirectory(root);
            var provider = new MultipartFormDataStreamProvider(root);

            var request = actionContext.Request;
            var content = request.Content;

            // Note: YES this is super strange, ugly, and weird.
            // One would think that you could just do:
            //
            //var result = content.ReadAsMultipartAsync(provider).Result;
            //
            // But it deadlocks. See https://stackoverflow.com/questions/15201255 for details, which
            // points to https://msdn.microsoft.com/en-us/magazine/jj991977.aspx which contains more
            // details under "Async All the Way" - see also https://olitee.com/2015/01/c-async-await-common-deadlock-scenario/
            // which contains a simplified explaination: ReadAsMultipartAsync is meant to be awaited,
            // not used in the non-async .Result way, and there is nothing we can do about it.
            //
            // Alas, model binders cannot be async "all the way", so we have to wrap in a task, to
            // force proper threading, and then it works.

            MultipartFormDataStreamProvider result = null;
            var task = Task.Run(() => content.ReadAsMultipartAsync(provider))
                           .ContinueWith(x =>
                           {
                               if (x.IsFaulted && x.Exception != null)
                               {
                                   throw x.Exception;
                               }
                               result = x.ConfigureAwait(false).GetAwaiter().GetResult();
                           });
            task.Wait();

            if (result == null)
                throw new InvalidOperationException("Could not read multi-part message");

            return result;
        }

        /// <summary>
        /// Builds the model from the request contents
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="bindingContext"></param>
        /// <param name="result"></param>
        /// <returns></returns>
        private TModelSave GetModel(HttpActionContext actionContext, ModelBindingContext bindingContext, MultipartFormDataStreamProvider result)
        {
            if (result.FormData["contentItem"] == null)
            {
                var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                response.ReasonPhrase = "The request was not formatted correctly and is missing the 'contentItem' parameter";
                throw new HttpResponseException(response);
            }

            //get the string json from the request
            var contentItem = result.FormData["contentItem"];

            //deserialize into our model
            var model = JsonConvert.DeserializeObject<TModelSave>(contentItem);

            //get the default body validator and validate the object
            var bodyValidator = actionContext.ControllerContext.Configuration.Services.GetBodyModelValidator();
            var metadataProvider = actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
            //all validation errors will not contain a prefix
            bodyValidator.Validate(model, typeof(TModelSave), metadataProvider, actionContext, "");

            //get the files
            foreach (var file in result.FileData)
            {
                //The name that has been assigned in JS has 2 parts and the second part indicates the property id
                // for which the file belongs.
                var parts = file.Headers.ContentDisposition.Name.Trim(new char[] { '\"' }).Split('_');
                if (parts.Length != 2)
                {
                    var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                    response.ReasonPhrase = "The request was not formatted correctly the file name's must be underscore delimited";
                    throw new HttpResponseException(response);
                }
                var propAlias = parts[1];

                var fileName = file.Headers.ContentDisposition.FileName.Trim(new char[] { '\"' });

                model.UploadedFiles.Add(new ContentPropertyFile
                {
                    TempFilePath = file.LocalFileName,
                    PropertyAlias = propAlias,
                    FileName = fileName
                });
            }

            if (ContentControllerBase.IsCreatingAction(model.Action))
            {
                //we are creating new content
                model.PersistedContent = CreateNew(model);
            }
            else
            {
                //finally, let's lookup the real content item and create the DTO item
                model.PersistedContent = GetExisting(model);
            }

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
        private static void MapPropertyValuesFromSaved(IContentProperties<ContentPropertyBasic> saveModel, ContentItemDto<TPersisted> dto)
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

        protected abstract TPersisted GetExisting(TModelSave model);
        protected abstract TPersisted CreateNew(TModelSave model);
        protected abstract ContentItemDto<TPersisted> MapFromPersisted(TModelSave model);
    }
}
