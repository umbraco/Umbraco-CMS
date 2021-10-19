// Copyright (c) Umbraco.
// See LICENSE for more details.

using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.TestHelpers.PublishedContent
{
    public class PublishedContentStrong2 : PublishedContentModel
    {
        public PublishedContentStrong2(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public int StrongValue => this.Value<int>(Mock.Of<IPublishedValueFallback>(), "strongValue");
    }
}
