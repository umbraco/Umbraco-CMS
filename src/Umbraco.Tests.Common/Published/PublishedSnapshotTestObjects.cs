// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using Moq;
using Umbraco.Core;
using Umbraco.Core.Models.PublishedContent;

namespace Umbraco.Tests.Published
{
    public class PublishedSnapshotTestObjects
    {
        [PublishedModel("element1")]
        public class TestElementModel1 : PublishedElementModel
        {
            public TestElementModel1(IPublishedElement content, IPublishedValueFallback fallback)
                : base(content)
            {
            }

            public string Prop1 => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop1");
        }

        [PublishedModel("element2")]
        public class TestElementModel2 : PublishedElementModel
        {
            public TestElementModel2(IPublishedElement content, IPublishedValueFallback fallback)
                : base(content)
            {
            }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>(Mock.Of<IPublishedValueFallback>(), "prop2");
        }

        [PublishedModel("content1")]
        public class TestContentModel1 : PublishedContentModel
        {
            public TestContentModel1(IPublishedContent content, IPublishedValueFallback fallback)
                : base(content)
            {
            }

            public string Prop1 => this.Value<string>(Mock.Of<IPublishedValueFallback>(), "prop1");
        }

        [PublishedModel("content2")]
        public class TestContentModel2 : PublishedContentModel
        {
            public TestContentModel2(IPublishedContent content, IPublishedValueFallback fallback)
                : base(content)
            {
            }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>(Mock.Of<IPublishedValueFallback>(), "prop2");
        }
    }
}
