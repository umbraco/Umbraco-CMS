// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.TestHelpers.PublishedContent
{
    public class PublishedContentStrong1Sub : PublishedContentStrong1
    {
        public PublishedContentStrong1Sub(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public int AnotherValue => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "anotherValue");
    }
}
