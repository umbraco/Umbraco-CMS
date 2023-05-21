// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Moq;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Extensions;

namespace Umbraco.Cms.Tests.Common.Published;

public class PublishedSnapshotTestObjects
{
    [PublishedModel("element1")]
    public class TestElementModel1 : PublishedElementModel
    {
        public TestElementModel1(IPublishedElement content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public string Prop1 => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop1");
    }

    [PublishedModel("element2")]
    public class TestElementModel2 : PublishedElementModel
    {
        public TestElementModel2(IPublishedElement content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public IEnumerable<TestContentModel1> Prop2 =>
            this.Value<IEnumerable<TestContentModel1>>(Mock.Of<IPublishedValueFallback>(), "prop2");
    }

    [PublishedModel("content1")]
    public class TestContentModel1 : PublishedContentModel
    {
        public TestContentModel1(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public string Prop1 => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop1");
    }

    [PublishedModel("content2")]
    public class TestContentModel2 : PublishedContentModel
    {
        public TestContentModel2(IPublishedContent content, IPublishedValueFallback fallback)
            : base(content, fallback)
        {
        }

        public IEnumerable<TestContentModel1> Prop2 =>
            this.Value<IEnumerable<TestContentModel1>>(Mock.Of<IPublishedValueFallback>(), "prop2");
    }
}
