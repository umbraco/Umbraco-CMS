using System;
using System.Web;
using Umbraco.Core.Models;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    /// <summary>
    /// Used for testing strongly-typed published content extensions that work against the <see cref="PublishedContentTests"/>
    /// </summary>
    public class Home : TypedModelBase
    {
        public Home(IPublishedContent publishedContent) : base(publishedContent)
        {
        }
    }
}
