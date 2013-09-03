using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    public class TextpageModel : TypedModelBase
    {
        public TextpageModel(IPublishedContent publishedContent) : base(publishedContent)
        {
        }

        public string Title { get { return ResolveString(ForThis()); } }

        public string BodyText { get { return ResolveString(ForThis()); } }

        public DateTime Date { get { return ResolveDate(ForThis()); } }
    }
}