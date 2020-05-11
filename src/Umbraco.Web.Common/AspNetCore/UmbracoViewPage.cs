using System;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Composing;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Strings;

namespace Umbraco.Web.Common.AspNetCore
{

    public abstract class UmbracoViewPage : UmbracoViewPage<IPublishedContent>
    {

    }

    public abstract class UmbracoViewPage<TModel> : RazorPage<TModel>
    {
        private IUmbracoContext _umbracoContext;
        private readonly IUmbracoContextAccessor _umbracoContextAccessor;
        private readonly IGlobalSettings _globalSettings;
        private readonly IContentSettings _contentSettings;
        private readonly IProfilerHtml _profilerHtml;

        protected IUmbracoContext UmbracoContext => _umbracoContext ??= _umbracoContextAccessor.UmbracoContext;

        protected UmbracoViewPage()
        {
            _umbracoContextAccessor = Context.RequestServices.GetRequiredService<IUmbracoContextAccessor>();
            _globalSettings = Context.RequestServices.GetRequiredService<IGlobalSettings>();
            _contentSettings = Context.RequestServices.GetRequiredService<IContentSettings>();
            _profilerHtml = Context.RequestServices.GetRequiredService<IProfilerHtml>();
        }


        public override void Write(object value)
        {
            if (value is IHtmlEncodedString htmlEncodedString)
            {
                base.WriteLiteral(htmlEncodedString.ToHtmlString());
            }
            else
            {
                base.Write(value);
            }
        }

        public override void WriteLiteral(object value)
        {
            // filter / add preview banner
            if (Context.Response.ContentType.InvariantEquals("text/html")) // ASP.NET default value
            {
                if (UmbracoContext.IsDebug || UmbracoContext.InPreviewMode)
                {
                    var text = value.ToString();
                    var pos = text.IndexOf("</body>", StringComparison.InvariantCultureIgnoreCase);

                    if (pos > -1)
                    {
                        string markupToInject;

                        if (UmbracoContext.InPreviewMode)
                        {
                            // creating previewBadge markup
                            markupToInject =
                                string.Format(_contentSettings.PreviewBadge,
                                    Current.IOHelper.ResolveUrl(_globalSettings.UmbracoPath),
                                    Context.Request.GetEncodedUrl(),
                                    UmbracoContext.PublishedRequest.PublishedContent.Id);
                        }
                        else
                        {
                            // creating mini-profiler markup
                            markupToInject = _profilerHtml.Render();
                        }

                        var sb = new StringBuilder(text);
                        sb.Insert(pos, markupToInject);

                        base.WriteLiteral(sb.ToString());
                        return;
                    }
                }
            }

            base.WriteLiteral(value);
        }
    }
}
