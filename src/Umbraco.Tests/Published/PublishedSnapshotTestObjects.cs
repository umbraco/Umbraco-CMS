using System;
using System.Collections.Generic;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Web;
using Umbraco.Web.PublishedCache;

namespace Umbraco.Tests.Published
{
    public class PublishedSnapshotTestObjects
    {
        [PublishedModel("element1")]
        public class TestElementModel1 : PublishedElementModel
        {
            public TestElementModel1(IPublishedElement content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedModel("element2")]
        public class TestElementModel2 : PublishedElementModel
        {
            public TestElementModel2(IPublishedElement content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }

        [PublishedModel("content1")]
        public class TestContentModel1 : PublishedContentModel
        {
            public TestContentModel1(IPublishedContent content)
                : base(content)
            { }

            public string Prop1 => this.Value<string>("prop1");
        }

        [PublishedModel("content2")]
        public class TestContentModel2 : PublishedContentModel
        {
            public TestContentModel2(IPublishedContent content)
                : base(content)
            { }

            public IEnumerable<TestContentModel1> Prop2 => this.Value<IEnumerable<TestContentModel1>>("prop2");
        }
        
    }
}
