// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Deploy
{
    [TestFixture]
    public class ArtifactBaseTests
    {
        [Test]
        public void CanSerialize()
        {
            var udi = new GuidUdi("test", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
            var artifact = new TestArtifact(udi, new List<ArtifactDependency>())
            {
                Name = "Test Name",
                Alias = "testAlias",
            };

            var serialized = JsonConvert.SerializeObject(artifact);

            var expected = "{\"Udi\":\"umb://test/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";
            Assert.AreEqual(expected, serialized);
        }

        private class TestArtifact : ArtifactBase<GuidUdi>
        {
            public TestArtifact(GuidUdi udi, IEnumerable<ArtifactDependency> dependencies = null) : base(udi, dependencies)
            {
            }

            protected override string GetChecksum() => "test checksum value";
        }
    }
}
