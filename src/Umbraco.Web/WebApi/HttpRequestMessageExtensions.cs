using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web.Http;
using System.Web.Http.ModelBinding;

namespace Umbraco.Web.WebApi
{

    public static class HttpRequestMessageExtensions
    {
        /// <summary>
        /// Create a 403 (Forbidden) response indicating that hte current user doesn't have access to the resource
        /// requested or the action it needs to take.
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        /// <remarks>
        /// This is different from a 401 which indicates that the user is not logged in.
        /// </remarks>
        public static HttpResponseMessage CreateUserNoAccessResponse(this HttpRequestMessage request)
        {
            return request.CreateResponse(HttpStatusCode.Forbidden);
        }

        /// <summary>
        /// Create a 400 response message indicating that a validation error occurred
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="request"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateValidationErrorResponse<T>(this HttpRequestMessage request, T value)
        {
            var msg = request.CreateResponse(HttpStatusCode.BadRequest, value);
            msg.Headers.Add("X-Status-Reason", "Validation failed");
            return msg;
        }

        /// <summary>
        /// Create a 400 response message indicating that a validation error occurred
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateValidationErrorResponse(this HttpRequestMessage request)
        {
            var msg = request.CreateResponse(HttpStatusCode.BadRequest);
            msg.Headers.Add("X-Status-Reason", "Validation failed");
            return msg;
        }

        /// <summary>
        /// Create a 400 response message indicating that a validation error occurred
        /// </summary>
        /// <param name="request"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateValidationErrorResponse(this HttpRequestMessage request, string errorMessage)
        {
            var msg = request.CreateErrorResponse(HttpStatusCode.BadRequest, errorMessage);
            msg.Headers.Add("X-Status-Reason", "Validation failed");
            return msg;
        }

        /// <summary>
        /// Create a 400 response message indicating that a validation error occurred
        /// </summary>
        /// <param name="request"></param>
        /// <param name="modelState"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateValidationErrorResponse(this HttpRequestMessage request, ModelStateDictionary modelState)
        {
            var msg = request.CreateErrorResponse(HttpStatusCode.BadRequest, modelState);
            msg.Headers.Add("X-Status-Reason", "Validation failed");
            return msg;
        }
    }

}
