using System.Globalization;
using Umbraco.Core.Models.PublishedContent;

// ReSharper disable once CheckNamespace
namespace Umbraco.Web.Models
{
    public class RenderModel<TContent> : RenderModel
        where TContent : IPublishedContent
    {
        public RenderModel(TContent content, CultureInfo culture)
            : base(content, culture)
        {
            Content = content;
        }

        public RenderModel(TContent content)
            : base(content)
        {
            Content = content;
        }

        public new TContent Content { get; private set; }
    }
}
