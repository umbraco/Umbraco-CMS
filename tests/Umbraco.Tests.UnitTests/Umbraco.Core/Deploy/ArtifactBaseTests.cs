// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Text.Json;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Deploy;
using Umbraco.Cms.Infrastructure.Serialization;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Core.Deploy;

/// <summary>
/// Contains unit tests for the ArtifactBase class.
/// </summary>
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

    /// <summary>
    /// Verifies that a TestArtifact instance is serialized to the expected JSON format.
    /// Ensures all properties are correctly included in the output.
    /// </summary>
    [Test]
    public void Can_Serialize()
    {
        var udi = new GuidUdi("document", Guid.Parse("3382d5433b5749d08919bc9961422a1f"));
        var artifact = new TestArtifact(udi, []) { Name = "Test Name", Alias = "testAlias" };

        string serialized = JsonSerializer.Serialize(artifact, _jsonSerializerOptions);

        var expected = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Checksum\":\"test checksum value\",\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";
        Assert.AreEqual(expected, serialized);
    }

    /// <summary>
    /// Tests that a TestArtifact object can be deserialized correctly from a JSON string.
    /// </summary>
    [Test]
    public void Can_Deserialize()
    {
        var serialized = "{\"Udi\":\"umb://document/3382d5433b5749d08919bc9961422a1f\",\"Dependencies\":[],\"Checksum\":\"test checksum value\",\"Name\":\"Test Name\",\"Alias\":\"testAlias\"}";

        TestArtifact? deserialized = JsonSerializer.Deserialize<TestArtifact>(serialized, _jsonSerializerOptions);
        Assert.IsNotNull(deserialized);
        Assert.AreEqual("Test Name", deserialized.Name);
        Assert.AreEqual("testAlias", deserialized.Alias);
    }

    /// <summary>
    /// Verifies that artifact dependencies are ordered consistently according to the defined rules,
    /// ensuring cross-platform consistency as required by issue #72.
    /// </summary>
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

    /// <summary>
    /// Verifies that when multiple dependencies with the same UDI but different modes are added to the collection, the mode is correctly updated according to the collection's logic.
    /// </summary>
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

        Assert.AreEqual(1, dependencies.Count);
        var dependency = dependencies.First();
        Assert.AreEqual(udi, dependency.Udi);
        Assert.AreEqual(false, dependency.Ordering);
        Assert.AreEqual(ArtifactDependencyMode.Match, dependency.Mode);
        Assert.AreEqual(null, dependency.Checksum);
    }

    /// <summary>
    /// Verifies that the <c>Dependencies</c> collection correctly updates its contents and maintains the expected ordering
    /// when multiple <see cref="ArtifactDependency"/> instances are added, including handling duplicates and ordering flags.
    /// Ensures that only the correct dependency remains with the appropriate properties set.
    /// </summary>
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

        Assert.AreEqual(1, dependencies.Count);
        var dependency = dependencies.First();
        Assert.AreEqual(udi, dependency.Udi);
        Assert.AreEqual(true, dependency.Ordering);
        Assert.AreEqual(ArtifactDependencyMode.Match, dependency.Mode);
        Assert.AreEqual(null, dependency.Checksum);
    }

    /// <summary>
    /// Tests that the dependencies collection correctly updates and maintains the checksum value.
    /// </summary>
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

        Assert.AreEqual(1, dependencies.Count);
        var dependency = dependencies.First();
        Assert.AreEqual(udi, dependency.Udi);
        Assert.AreEqual(true, dependency.Ordering);
        Assert.AreEqual(ArtifactDependencyMode.Match, dependency.Mode);
        Assert.AreEqual("123", dependency.Checksum);
    }

    private class TestArtifact : ArtifactBase<GuidUdi>
    {
    /// <summary>
    /// Initializes a new instance of the <see cref="TestArtifact"/> class.
    /// </summary>
    /// <param name="udi">The unique identifier for the artifact.</param>
    /// <param name="dependencies">The dependencies of the artifact.</param>
        public TestArtifact(GuidUdi udi, IEnumerable<ArtifactDependency> dependencies = null)
            : base(udi, dependencies)
        {
        }

        protected override string GetChecksum() => "test checksum value";
    }
}
