// Copyright (c) Umbraco.
// See LICENSE for more details.

using Umbraco.Cms.Core.Models.PublishedContent;

namespace Umbraco.Cms.Tests.Common.TestHelpers.PublishedContent
{
    [PublishedModel("ContentType2Sub")]
    public class ContentType2Sub : ContentType2
    {
        public ContentType2Sub(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }
    }
}
