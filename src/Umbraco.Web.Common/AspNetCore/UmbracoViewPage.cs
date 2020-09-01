using System;
using System.Text;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.IO;
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
        private IUmbracoContextAccessor UmbracoContextAccessor => Context.RequestServices.GetRequiredService<IUmbracoContextAccessor>();
        private GlobalSettings GlobalSettings => Context.RequestServices.GetRequiredService<IOptions<GlobalSettings>>().Value;
        private ContentSettings ContentSettings => Context.RequestServices.GetRequiredService<IOptions<ContentSettings>>().Value;
        private IProfilerHtml ProfilerHtml => Context.RequestServices.GetRequiredService<IProfilerHtml>();
        private IIOHelper IOHelper => Context.RequestServices.GetRequiredService<IIOHelper>();

        protected IUmbracoContext UmbracoContext => _umbracoContext ??= UmbracoContextAccessor.UmbracoContext;


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
                                string.Format(ContentSettings.PreviewBadge,
                                    IOHelper.ResolveUrl(GlobalSettings.UmbracoPath),
                                    Context.Request.GetEncodedUrl(),
                                    UmbracoContext.PublishedRequest.PublishedContent.Id);
                        }
                        else
                        {
                            // creating mini-profiler markup
                            markupToInject = ProfilerHtml.Render();
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
