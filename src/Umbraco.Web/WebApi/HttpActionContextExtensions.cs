using System;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.Controllers;
using Newtonsoft.Json;
using Umbraco.Core.IO;

namespace Umbraco.Web.WebApi
{
    internal static class HttpActionContextExtensions
    {
        /// <summary>
        /// Helper method to get a model from a multipart request and ensure that the model is validated
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="actionContext"></param>
        /// <param name="result"></param>
        /// <param name="requestKey"></param>
        /// <param name="validationKeyPrefix"></param>
        /// <returns></returns>
        public static T GetModelFromMultipartRequest<T>(this HttpActionContext actionContext, MultipartFormDataStreamProvider result, string requestKey, string validationKeyPrefix = "")
        {
            if (result.FormData[requestKey/*"contentItem"*/] == null)
            {
                var response = actionContext.Request.CreateResponse(HttpStatusCode.BadRequest);
                response.ReasonPhrase = $"The request was not formatted correctly and is missing the '{requestKey}' parameter";
                throw new HttpResponseException(response);
            }

            //get the string json from the request
            var contentItem = result.FormData[requestKey];

            //deserialize into our model
            var model = JsonConvert.DeserializeObject<T>(contentItem);

            //get the default body validator and validate the object
            var bodyValidator = actionContext.ControllerContext.Configuration.Services.GetBodyModelValidator();
            var metadataProvider = actionContext.ControllerContext.Configuration.Services.GetModelMetadataProvider();
            //by default all validation errors will not contain a prefix (empty string) unless specified
            bodyValidator.Validate(model, typeof(T), metadataProvider, actionContext, validationKeyPrefix);

            return model;
        }

        /// <summary>
        /// Helper method to get the <see cref="MultipartFormDataStreamProvider"/> from the request in a non-async manner
        /// </summary>
        /// <param name="actionContext"></param>
        /// <param name="rootVirtualPath"></param>
        /// <returns></returns>
        public static MultipartFormDataStreamProvider ReadAsMultipart(this HttpActionContext actionContext, string rootVirtualPath)
        {
            if (actionContext.Request.Content.IsMimeMultipartContent() == false)
            {
                throw new HttpResponseException(HttpStatusCode.UnsupportedMediaType);
            }

            var root = IOHelper.MapPath(rootVirtualPath);
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
            // which contains a simplified explanation: ReadAsMultipartAsync is meant to be awaited,
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
                },
                // Must explicitly specify this, see https://blog.stephencleary.com/2013/10/continuewith-is-dangerous-too.html
                TaskScheduler.Default);
            task.Wait();

            if (result == null)
                throw new InvalidOperationException("Could not read multi-part message");

            return result;
        }
    }
}
