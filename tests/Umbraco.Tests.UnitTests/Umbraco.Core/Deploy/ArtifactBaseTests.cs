// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Deploy;

[TestFixture]
public class ArtifactBaseTests
{
    [Test]
    public void CanSerialize()
    {
        var udi = new GuidUdi("test", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
        var artifact = new TestArtifact(udi, new List<ArtifactDependency>()) { Name = "Test Name", Alias = "testAlias" };

        var serialized = JsonConvert.SerializeObject(artifact);

        var expected =
            "{\"Udi\":\"umb://test/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";
        Assert.AreEqual(expected, serialized);
    }

    [Test]
    public void Dependencies_Are_Correctly_Ordered()
    {
        // This test was introduced following: https://github.com/umbraco/Umbraco.Deploy.Issues/issues/72 to verify
        // that consistent ordering rules are used across platforms.
        var udi = new GuidUdi("test", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
        var artifact = new TestArtifact(udi, new List<ArtifactDependency>()) { Name = "Test Name", Alias = "testAlias" };

        var dependencies = new ArtifactDependencyCollection();

        var dependencyUdi1 = new GuidUdi("template", Guid.Parse("d4651496fad24c1290a53ea4d55d945b"));
        dependencies.Add(new ArtifactDependency(dependencyUdi1, true, ArtifactDependencyMode.Exist));

        var dependencyUdi2 = new StringUdi(Constants.UdiEntityType.TemplateFile, "TestPage.cshtml");
        dependencies.Add(new ArtifactDependency(dependencyUdi2, true, ArtifactDependencyMode.Exist));

        artifact.Dependencies = dependencies;

        Assert.AreEqual(
            "umb://template-file/TestPage.cshtml,umb://template/d4651496fad24c1290a53ea4d55d945b",
            string.Join(",", artifact.Dependencies.Select(x => x.Udi.ToString())));
    }

    private class TestArtifact : ArtifactBase<GuidUdi>
    {
        public TestArtifact(GuidUdi udi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, dependencies)
        {
        }

        protected override string GetChecksum() => "test checksum value";
    }
}
