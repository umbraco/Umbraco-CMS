// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Deploy;

[TestFixture]
public class ArtifactBaseTests
{
    private readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions
        {
            Converters =
            {
                new JsonUdiConverter(),
            }
        };

    [Test]
    public void Can_Serialize()
    {
        var udi = new GuidUdi("document", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
        var artifact = new TestArtifact(udi, []) { Name = "Test Name", Alias = "testAlias" };

        string serialized = JsonSerializer.Serialize(artifact, _jsonSerializerOptions);

        var expected = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Checksum\":\"test checksum value\",\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";
        Assert.That(serialized, Is.EqualTo(expected));
    }

    [Test]
    public void Can_Deserialize()
    {
        var serialized = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Checksum\":\"test checksum value\",\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";

        TestArtifact? deserialized = JsonSerializer.Deserialize<TestArtifact>(serialized, _jsonSerializerOptions);
        Assert.That(deserialized, Is.Not.Null);
        Assert.That(deserialized.Name, Is.EqualTo("Test Name"));
        Assert.That(deserialized.Alias, Is.EqualTo("testAlias"));
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

        Assert.That(
            string.Join(",", artifact.Dependencies.Select(x => x.Udi.ToString())), Is.EqualTo("umb://template-file/TestPage.cshtml,umb://template/d4651496fad24c1290a53ea4d55d945b"));
    }

    [Test]
    public void Dependencies_Correctly_Updates_Mode()
    {
        var udi = Udi.Create(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

        var dependencies = new ArtifactDependencyCollection
        {
            // Keep Match
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Match),
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist),
        };

        Assert.That(dependencies, Has.Count.EqualTo(1));
        var dependency = dependencies.First();
        Assert.That(dependency.Udi, Is.EqualTo(udi));
        Assert.That(dependency.Ordering, Is.EqualTo(false));
        Assert.That(dependency.Mode, Is.EqualTo(ArtifactDependencyMode.Match));
        Assert.That(dependency.Checksum, Is.EqualTo(null));
    }

    [Test]
    public void Dependencies_Correctly_Updates_Ordering()
    {
        var udi = Udi.Create(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

        var dependencies = new ArtifactDependencyCollection
        {
            // Keep ordering (regardless of mode)
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Match),
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist),
            new ArtifactDependency(udi, true, ArtifactDependencyMode.Match),
            new ArtifactDependency(udi, true, ArtifactDependencyMode.Exist),
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Match),
            new ArtifactDependency(udi, false, ArtifactDependencyMode.Exist),
        };

        Assert.That(dependencies, Has.Count.EqualTo(1));
        var dependency = dependencies.First();
        Assert.That(dependency.Udi, Is.EqualTo(udi));
        Assert.That(dependency.Ordering, Is.EqualTo(true));
        Assert.That(dependency.Mode, Is.EqualTo(ArtifactDependencyMode.Match));
        Assert.That(dependency.Checksum, Is.EqualTo(null));
    }

    [Test]
    public void Dependencies_Correctly_Updates_Checksum()
    {
        var udi = Udi.Create(Constants.UdiEntityType.AnyGuid, Guid.NewGuid());

        var dependencies = new ArtifactDependencyCollection
        {
            // Keep checksum
            new ArtifactDependency(udi, true, ArtifactDependencyMode.Match, "123"),
            new ArtifactDependency(udi, true, ArtifactDependencyMode.Match, string.Empty),
            new ArtifactDependency(udi, true, ArtifactDependencyMode.Match),
        };

        Assert.That(dependencies, Has.Count.EqualTo(1));
        var dependency = dependencies.First();
        Assert.That(dependency.Udi, Is.EqualTo(udi));
        Assert.That(dependency.Ordering, Is.EqualTo(true));
        Assert.That(dependency.Mode, Is.EqualTo(ArtifactDependencyMode.Match));
        Assert.That(dependency.Checksum, Is.EqualTo("123"));
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
