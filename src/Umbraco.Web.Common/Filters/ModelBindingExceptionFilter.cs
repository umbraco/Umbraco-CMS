using System;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web.Common.ModelBinders;

namespace Umbraco.Web.Common.Filters
{
    /// <summary>
    ///     An exception filter checking if we get a <see cref="ModelBindingException" /> or <see cref="InvalidCastException" /> with the same model.
    ///     In which case it returns a redirect to the same page after 1 sec if not in debug mode.
    /// </summary>
    /// <remarks>
    /// This is only enabled when running PureLive
    /// </remarks>
    public class ModelBindingExceptionFilter : ActionFilterAttribute, IExceptionFilter
    {
        private static readonly Regex _getPublishedModelsTypesRegex = new Regex("Umbraco.Web.PublishedModels.(\\w+)", RegexOptions.Compiled);

        private readonly ExceptionFilterSettings _exceptionFilterSettings;
        private readonly IPublishedModelFactory _publishedModelFactory;

        public ModelBindingExceptionFilter(IOptions<ExceptionFilterSettings> exceptionFilterSettings, IPublishedModelFactory publishedModelFactory)
        {
            _exceptionFilterSettings = exceptionFilterSettings.Value;
            _publishedModelFactory = publishedModelFactory ?? throw new ArgumentNullException(nameof(publishedModelFactory));
        }

        public void OnException(ExceptionContext filterContext)
        {
            var disabled = _exceptionFilterSettings?.Disabled ?? false;
            if (_publishedModelFactory.IsLiveFactory()
                && !disabled
                && !filterContext.ExceptionHandled
                && ((filterContext.Exception is ModelBindingException || filterContext.Exception is InvalidCastException)
                    && IsMessageAboutTheSameModelType(filterContext.Exception.Message)))
            {
                filterContext.HttpContext.Response.Headers.Add(HttpResponseHeader.RetryAfter.ToString(), "1");
                filterContext.Result = new RedirectResult(filterContext.HttpContext.Request.GetEncodedUrl(), false);

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
            var matches = _getPublishedModelsTypesRegex.Matches(exceptionMessage);

            if (matches.Count >= 2)
            {
                return string.Equals(matches[0].Value, matches[1].Value, StringComparison.InvariantCulture);
            }

            return false;
        }
    }
}
