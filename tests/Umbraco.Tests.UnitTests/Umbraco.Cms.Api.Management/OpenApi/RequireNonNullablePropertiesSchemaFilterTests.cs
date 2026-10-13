using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;
using NUnit.Framework;
using Swashbuckle.AspNetCore.SwaggerGen;
using Umbraco.Cms.Api.Management.OpenApi;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Cms.Api.Management.OpenApi;

/// <summary>
/// Verifies that <see cref="RequireNonNullablePropertiesSchemaFilter"/> lists exactly the non-nullable properties of a
/// model as required.
/// </summary>
/// <remarks>
/// The schemas are produced by Swashbuckle's own <see cref="SchemaGenerator"/> rather than built by hand, so the filter
/// is tested against the schema shapes Swashbuckle actually emits. Nullability is represented differently per shape
/// (a type flag, a <c>oneOf</c> with a null type, or nothing at all for a schema without a type), and
/// <see cref="TestModel"/> covers each shape in both its nullable and non-nullable form.
/// </remarks>
[TestFixture]
internal sealed class RequireNonNullablePropertiesSchemaFilterTests
{
    [Test]
    public void Non_Nullable_Properties_Are_Required()
    {
        ISet<string> required = GenerateRequired<TestModel>();

        // Other properties may be required too (see Explicitly_Required_Nullable_Properties_Remain_Required), hence a superset.
        Assert.That(required, Is.SupersetOf(new[] { "name", "reference", "value", "count", "referenceList" }));
    }

    // One case per schema shape: a typed schema, a oneOf-wrapped reference, a schema without a type, a value type and an array.
    [TestCase("description")]
    [TestCase("nullableReference")]
    [TestCase("nullableValue")]
    [TestCase("nullableCount")]
    [TestCase("nullableReferenceList")]
    public void Nullable_Properties_Are_Not_Required(string propertyName)
    {
        ISet<string> required = GenerateRequired<TestModel>();

        Assert.That(required, Does.Not.Contain(propertyName));
    }

    [Test]
    public void Explicitly_Required_Nullable_Properties_Remain_Required()
    {
        ISet<string> required = GenerateRequired<TestModel>();

        // Swashbuckle marks C# "required" members as required itself; the filter must not remove them because they are nullable.
        Assert.That(required, Does.Contain("requiredNullableReference"));
    }

    /// <summary>
    /// Generates the schema for <typeparamref name="T"/> with the filter applied, and returns its required property names.
    /// </summary>
    private static ISet<string> GenerateRequired<T>()
    {
        // Mirror the options the Management API configures that affect how nullability is represented.
        var options = new SchemaGeneratorOptions
        {
            SupportNonNullableReferenceTypes = true,
            UseOneOfForPolymorphism = true,

            // Selecting the type itself as its only sub type wraps references in a oneOf, as the Management API does.
            SubTypesSelector = type => type == typeof(TestReference) ? [type] : [],
        };

        // The resolver needs the generator options too, otherwise it ignores nullable reference type annotations.
        var dataContractResolver = new JsonSerializerDataContractResolver(new JsonSerializerOptions(JsonSerializerDefaults.Web), options);

        // The filter resolves the data contract resolver from the service provider, as it does when Swashbuckle creates it.
        IServiceProvider serviceProvider = new ServiceCollection()
            .AddSingleton<ISerializerDataContractResolver>(dataContractResolver)
            .BuildServiceProvider();
        options.SchemaFilters.Add(new RequireNonNullablePropertiesSchemaFilter(serviceProvider));

        var schemaRepository = new SchemaRepository();
        new SchemaGenerator(options, dataContractResolver).GenerateSchema(typeof(T), schemaRepository);

        // GenerateSchema returns a reference; the model's own schema is registered in the repository.
        return schemaRepository.Schemas[typeof(T).Name].Required ?? new HashSet<string>();
    }

    private sealed class TestReference
    {
        public Guid Id { get; set; }
    }

    /// <summary>
    /// A model with each schema shape in both a nullable and a non-nullable form. Property names are camel-cased in the schema.
    /// </summary>
    private sealed class TestModel
    {
        // Typed schema ("type": "string"); nullability is carried by the type itself.
        public string Name { get; set; } = string.Empty;

        public string? Description { get; set; }

        // Reference to another schema, wrapped in a oneOf; when nullable, the oneOf also carries a null type.
        public TestReference Reference { get; set; } = new();

        public TestReference? NullableReference { get; set; }

        public required TestReference? RequiredNullableReference { get; set; }

        // Schema without a type ({ }); the schema carries no nullability, so it comes from the C# declaration.
        public object Value { get; set; } = new();

        public object? NullableValue { get; set; }

        // Value type, where nullability comes from Nullable<T> rather than an annotation.
        public int Count { get; set; }

        public int? NullableCount { get; set; }

        // Array schema ("type": "array").
        public List<TestReference> ReferenceList { get; set; } = [];

        public List<TestReference>? NullableReferenceList { get; set; }
    }
}
