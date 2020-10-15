using System;
using System.Configuration;
using System.Net;
using System.Text.RegularExpressions;
using System.Web.Mvc;
using Microsoft.Extensions.Options;
using Umbraco.Core.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Web.Composing;

namespace Umbraco.Web.Mvc
{
    /// <summary>
    ///     An exception filter checking if we get a <see cref="ModelBindingException" /> or <see cref="InvalidCastException" /> with the same model.
    ///     In which case it returns a redirect to the same page after 1 sec if not in debug mode.
    /// </summary>
    /// <remarks>
    /// This is only enabled when running PureLive
    /// </remarks>
    /// Migrated to .NET Core
    internal class ModelBindingExceptionFilter : FilterAttribute, IExceptionFilter
    {
        private static readonly Regex GetPublishedModelsTypesRegex = new Regex("Umbraco.Web.PublishedModels.(\\w+)", RegexOptions.Compiled);

        public void OnException(ExceptionContext filterContext)
        {
            var settings = Current.Factory.GetInstance<IOptions<ExceptionFilterSettings>>().Value;
            var disabled = settings?.Disabled ?? false;
            if (Current.PublishedModelFactory.IsLiveFactory()
                && !disabled
                && !filterContext.ExceptionHandled
                && ((filterContext.Exception is ModelBindingException || filterContext.Exception is InvalidCastException)
                    && IsMessageAboutTheSameModelType(filterContext.Exception.Message)))
            {
                filterContext.HttpContext.Response.Headers.Add(HttpResponseHeader.RetryAfter.ToString(), "1");
                filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.RawUrl, false);

                filterContext.ExceptionHandled = true;
            }
        }

        /// <summary>
        ///     Returns true if the message is about two models with the same name.
        /// </summary>
        /// <remarks>
        ///     Message could be something like:
        /// <para>
        ///     InvalidCastException:
        ///     [A]Umbraco.Web.PublishedModels.Home cannot be cast to [B]Umbraco.Web.PublishedModels.Home. Type A originates from 'App_Web_all.generated.cs.8f9494c4.rtdigm_z, Version=0.0.0.3, Culture=neutral, PublicKeyToken=null' in the context 'Default' at location 'C:\Users\User\AppData\Local\Temp\Temporary ASP.NET Files\root\c5c63f4d\c168d9d4\App_Web_all.generated.cs.8f9494c4.rtdigm_z.dll'. Type B originates from 'App_Web_all.generated.cs.8f9494c4.rbyqlplu, Version=0.0.0.5, Culture=neutral, PublicKeyToken=null' in the context 'Default' at location 'C:\Users\User\AppData\Local\Temp\Temporary ASP.NET Files\root\c5c63f4d\c168d9d4\App_Web_all.generated.cs.8f9494c4.rbyqlplu.dll'.
        ///</para>
        /// <para>
        ///     ModelBindingException:
        ///     Cannot bind source content type Umbraco.Web.PublishedModels.Home to model type Umbraco.Web.PublishedModels.Home. Both view and content models are PureLive, with different versions. The application is in an unstable state and is going to be restarted. The application is restarting now.
        /// </para>
        /// </remarks>
        private bool IsMessageAboutTheSameModelType(string exceptionMessage)
        {
            var matches = GetPublishedModelsTypesRegex.Matches(exceptionMessage);

            if (matches.Count >= 2)
            {
                return string.Equals(matches[0].Value, matches[1].Value, StringComparison.InvariantCulture);
            }

            return false;
        }
    }
}
