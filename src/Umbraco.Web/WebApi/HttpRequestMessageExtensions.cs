﻿using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web;
using System.Web.Http.ModelBinding;
using Microsoft.Owin;
using Umbraco.Core;
using Umbraco.Web.Models.ContentEditing;

namespace Umbraco.Web.WebApi
{

    public static class HttpRequestMessageExtensions
    {

        /// <summary>
        /// Borrowed from the latest Microsoft.AspNet.WebApi.Owin package which we cannot use because of a later webapi dependency
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        internal static Attempt<IOwinContext> TryGetOwinContext(this HttpRequestMessage request)
        {
            // occurs in unit tests?
            if (request.Properties.TryGetValue("MS_OwinContext", out var o) && o is IOwinContext owinContext)
                return Attempt.Succeed(owinContext);

            var httpContext = request.TryGetHttpContext();
            try
            {
                return httpContext
                        ? Attempt.Succeed(httpContext.Result.GetOwinContext())
                        : Attempt<IOwinContext>.Fail();
            }
            catch (InvalidOperationException)
            {
                //this will occur if there is no OWIN environment which generally would only be in things like unit tests
                return Attempt<IOwinContext>.Fail();
            }
        }

        /// <summary>
        /// Tries to retrieve the current HttpContext if one exists.
        /// </summary>
        /// <returns></returns>
        public static Attempt<HttpContextBase> TryGetHttpContext(this HttpRequestMessage request)
        {
            object context;
            if (request.Properties.TryGetValue("MS_HttpContext", out context))
            {
                var httpContext = context as HttpContextBase;
                if (httpContext != null)
                {
                    return Attempt.Succeed(httpContext);
                }
            }
            if (HttpContext.Current != null)
            {
                return Attempt<HttpContextBase>.Succeed(new HttpContextWrapper(HttpContext.Current));
            }

            return Attempt<HttpContextBase>.Fail();
        }

        /// <summary>
        /// Create a 403 (Forbidden) response indicating that the current user doesn't have access to the resource
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
        /// Creates an error response with notifications in the result to be displayed in the UI
        /// </summary>
        /// <param name="request"></param>
        /// <param name="errorMessage"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateNotificationValidationErrorResponse(this HttpRequestMessage request, string errorMessage)
        {
            var notificationModel = new SimpleNotificationModel
            {
                Message = errorMessage
            };
            notificationModel.AddErrorNotification(errorMessage, string.Empty);
            return request.CreateValidationErrorResponse(notificationModel);
        }

        /// <summary>
        /// Creates a successful response with notifications in the result to be displayed in the UI
        /// </summary>
        /// <param name="request"></param>
        /// <param name="successMessage"></param>
        /// <returns></returns>
        public static HttpResponseMessage CreateNotificationSuccessResponse(this HttpRequestMessage request, string successMessage)
        {
            var notificationModel = new SimpleNotificationModel
            {
                Message = successMessage
            };
            notificationModel.AddSuccessNotification(successMessage, string.Empty);
            return request.CreateResponse(HttpStatusCode.OK, notificationModel);
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

        internal static string ClientCulture(this HttpRequestMessage request)
        {
            return request.Headers.TryGetValues("X-UMB-CULTURE", out var values) ? values.FirstOrDefault() : null;
        }
    }

}
