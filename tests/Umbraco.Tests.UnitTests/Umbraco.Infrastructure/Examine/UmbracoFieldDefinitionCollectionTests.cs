using Examine;
using NUnit.Framework;
using Umbraco.Cms.Infrastructure.Examine;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Examine;

[TestFixture]
internal class UmbracoFieldDefinitionCollectionTests
{
    [Test]
    public void Create_Contains_Expected_Fields()
    {
        var collection = new UmbracoFieldDefinitionCollection();
        AssertDefaultField(collection);
    }

    [Test]
    public void Create_New_Contains_Expected_Fields()
    {
        var collection = new UmbracoFieldDefinitionCollection();
        collection.AddOrUpdate(new FieldDefinition("customField", "string"));
        var collectionCount = collection.Count;

        collection = new UmbracoFieldDefinitionCollection();
        Assert.That(collection, Has.Count.EqualTo(collectionCount - 1));
        AssertDefaultField(collection);
        AssertCustomField(collection, expectExists: false);
    }

    [Test]
    public void Create_With_Existing_Contains_Expected_Fields()
    {
        var collection = new UmbracoFieldDefinitionCollection();
        collection.AddOrUpdate(new FieldDefinition("customField", "string"));
        var collectionCount = collection.Count;

        collection = new UmbracoFieldDefinitionCollection(collection);
        Assert.That(collection, Has.Count.EqualTo(collectionCount));
        AssertDefaultField(collection);
        AssertCustomField(collection, expectExists: true);
    }

    [Test]
    public void Create_With_Existing_Retains_Override_Of_DefaultField()
    {
        var collection = new UmbracoFieldDefinitionCollection();
        collection.AddOrUpdate(new FieldDefinition("parentID", "string"));

        collection = new UmbracoFieldDefinitionCollection(collection);
        AssertDefaultField(collection, "string");
    }

    private static void AssertDefaultField(UmbracoFieldDefinitionCollection collection, string expectedType = "int")
    {
        var field = collection.SingleOrDefault(x => x.Name == "parentID");
        Assert.That(field, Is.Not.Null);
        Assert.That(field.Name, Is.EqualTo("parentID"));
        Assert.That(field.Type, Is.EqualTo(expectedType));
    }

    private static void AssertCustomField(UmbracoFieldDefinitionCollection collection, bool expectExists)
    {
        var field = collection.SingleOrDefault(x => x.Name == "customField");
        if (expectExists is false)
        {
            Assert.That(field.Name, Is.Null);
            Assert.That(field.Type, Is.Null);
            return;
        }

        Assert.That(field, Is.Not.Null);
        Assert.That(field.Name, Is.EqualTo("customField"));
        Assert.That(field.Type, Is.EqualTo("string"));
    }
}
