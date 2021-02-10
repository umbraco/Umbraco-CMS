// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Diagnostics;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Tests.Common.Builders;
using Umbraco.Cms.Tests.Common.Builders.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Models
{
    [TestFixture]
    public class DocumentEntityTests
    {
        private DocumentEntitySlimBuilder _builder;

        [SetUp]
        public void SetUp() => _builder = new DocumentEntitySlimBuilder();

        [Test]
        public void Can_Serialize_Without_Error()
        {
            DocumentEntitySlim item = _builder
                .WithId(3)
                .WithCreatorId(4)
                .WithName("Test")
                .WithParentId(5)
                .WithSortOrder(6)
                .WithLevel(7)
                .WithContentTypeAlias("test1")
                .WithContentTypeIcon("icon")
                .WithContentTypeThumbnail("thumb")
                .WithHasChildren(true)
                .WithPublished(true)
                .AddAdditionalData()
                    .WithKeyValue("test1", 3)
                    .WithKeyValue("test2", "value")
                    .Done()
                .Build();

            var json = JsonConvert.SerializeObject(item);
            Debug.Print(json);
        }
    }
}
