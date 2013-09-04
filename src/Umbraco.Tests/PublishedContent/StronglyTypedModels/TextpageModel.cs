using System;
using Umbraco.Core.Models;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    public class TextpageModel : TypedModelBase
    {
        public TextpageModel(IPublishedContent publishedContent) : base(publishedContent)
        {
        }

        public string Title { get { return Resolve(Property(), DefaultString); } }

        public string BodyText { get { return Resolve(Property(), DefaultString); } }

        public string AuthorName { get { return Resolve<string>(Property()); } }

        public DateTime Date { get { return Resolve(Property(), DefaultDateTime); } }
    }
}