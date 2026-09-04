// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Moq;
using NUnit.Framework;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Models.PublishedContent;
using Umbraco.Cms.Core.PropertyEditors;
using Umbraco.Cms.Core.PropertyEditors.DeliveryApi;
using Umbraco.Cms.Core.PropertyEditors.ValueConverters;
using Umbraco.Cms.Infrastructure.PropertyEditors.ValueConverters;
using Umbraco.Cms.Tests.Common.Testing;
using Umbraco.Cms.Tests.Integration.Testing;

namespace Umbraco.Cms.Tests.Integration.Umbraco.Core.PropertyEditors;

/// <summary>
/// Asserts that the model type of a property is a property of its editor, and not of the configuration of the data
/// type it is used through.
/// </summary>
/// <remarks>
/// <para>
/// A value converter that derives its type from configuration turns an ordinary configuration edit - raising a
/// maximum number of items, ticking a "multiple" toggle - into a change of the model every template and Delivery API
/// consumer of that property sees. Where both models are genuinely wanted, the editor is split in two instead, as
/// <c>Umbraco.SingleBlock</c> was split out of <c>Umbraco.BlockList</c>.
/// </para>
/// <para>
/// The exempt converters below are the whole of the permitted variance, and the list is asserted exactly: a new
/// offender fails, and so does an entry that has since been fixed.
/// </para>
/// </remarks>
[TestFixture]
[UmbracoTest(Database = UmbracoTestOptions.Database.None)]
internal sealed class PropertyValueConverterConfigurationInvarianceTests : UmbracoIntegrationTest
{
    /// <summary>
    /// Converters whose model type legitimately depends on configuration.
    /// </summary>
    /// <remarks>
    /// The block converters name the element types their generic arguments come from, which is what a block editor's
    /// configuration is. The multi node tree picker is deprecated in favour of the dedicated document, media, element
    /// and member pickers, and its model is frozen as it stands rather than migrated.
    /// </remarks>
    private static readonly Type[] _exemptConverters =
    [
        typeof(BlockListPropertyValueConverter),
        typeof(BlockGridPropertyValueConverter),
        typeof(SingleBlockPropertyValueConverter),
        typeof(MultiNodeTreePickerValueConverter),
    ];

    /// <summary>
    /// Values whose meaningful domain cannot be discovered by reflection, so the permutations have to be told.
    /// </summary>
    private static readonly Dictionary<string, object?[]> _constrainedDomains = new()
    {
        [$"{nameof(MultiNodePickerConfigurationTreeSource)}.{nameof(MultiNodePickerConfigurationTreeSource.ObjectType)}"] =
            StringConstantsOf(typeof(Constants.UdiEntityType)),
    };

    private PropertyEditorCollection PropertyEditors => GetRequiredService<PropertyEditorCollection>();

    private PropertyValueConverterCollection PropertyValueConverters
        => GetRequiredService<PropertyValueConverterCollection>();

    [Test]
    public void Model_Type_Does_Not_Depend_On_Data_Type_Configuration()
    {
        var variantEditorAliases = VariantEditorAliases(
            (converter, propertyType) => converter.GetPropertyValueType(propertyType));

        Assert.That(variantEditorAliases, Is.Empty);
    }

    [Test]
    public void Delivery_Api_Model_Type_Does_Not_Depend_On_Data_Type_Configuration()
    {
        var variantEditorAliases = VariantEditorAliases((converter, propertyType) =>
            converter is IDeliveryApiPropertyValueConverter deliveryApiConverter
                ? deliveryApiConverter.GetDeliveryApiPropertyValueType(propertyType)
                : null);

        Assert.That(variantEditorAliases, Is.Empty);
    }

    /// <summary>
    /// Gets the aliases of the editors for which <paramref name="modelType"/> is not the same for every configuration
    /// the editor can be given.
    /// </summary>
    private List<string> VariantEditorAliases(Func<IPropertyValueConverter, IPublishedPropertyType, Type?> modelType)
    {
        var variantEditorAliases = new List<string>();

        foreach (IDataEditor editor in PropertyEditors)
        {
            Type? configurationType = ConfigurationType(editor);
            if (configurationType is null)
            {
                // the editor has no typed configuration, so there is nothing a converter could branch on
                continue;
            }

            foreach (IPropertyValueConverter converter in ConvertersFor(editor))
            {
                if (_exemptConverters.Contains(converter.GetType()))
                {
                    continue;
                }

                var modelTypesByConfiguration = new Dictionary<string, Type?>();
                foreach ((var description, var configuration) in Permutations(configurationType))
                {
                    modelTypesByConfiguration[description] =
                        modelType(converter, PropertyType(editor.Alias, configuration));
                }

                if (modelTypesByConfiguration.Values.Distinct().Count() > 1)
                {
                    variantEditorAliases.Add(editor.Alias);
                    TestContext.Out.WriteLine(
                        $"{editor.Alias} ({converter.GetType().Name}): "
                        + string.Join(", ", modelTypesByConfiguration.Select(pair => $"[{pair.Key}] => {pair.Value?.Name ?? "none"}")));
                }
            }
        }

        return variantEditorAliases;
    }

    /// <summary>
    /// Gets every converter that claims the editor, rather than only the one that would win, as any of them could be
    /// the one in play once shadowing is taken into account.
    /// </summary>
    private IEnumerable<IPropertyValueConverter> ConvertersFor(IDataEditor editor)
    {
        IPublishedPropertyType propertyType = PropertyType(editor.Alias, configuration: null);
        return PropertyValueConverters.Where(converter => converter.IsConverter(propertyType)).ToArray();
    }

    private static IPublishedPropertyType PropertyType(string editorAlias, object? configuration)
    {
        var dataType = new PublishedDataType(1, editorAlias, editorAlias, new Lazy<object?>(() => configuration));

        var propertyType = new Mock<IPublishedPropertyType>();
        propertyType.SetupGet(x => x.Alias).Returns("test");
        propertyType.SetupGet(x => x.EditorAlias).Returns(editorAlias);
        propertyType.SetupGet(x => x.DataType).Returns(dataType);

        return propertyType.Object;
    }

    /// <summary>
    /// Gets the configuration type of the editor, being the type argument of its
    /// <see cref="ConfigurationEditor{TConfiguration}"/>, or null when it has none.
    /// </summary>
    private static Type? ConfigurationType(IDataEditor editor)
    {
        for (Type? type = editor.GetConfigurationEditor().GetType(); type is not null; type = type.BaseType)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(ConfigurationEditor<>))
            {
                return type.GetGenericArguments()[0];
            }
        }

        return null;
    }

    /// <summary>
    /// Gets the configuration objects to test the editor with: the default, then one per configuration value the
    /// editor exposes, set to each of the values that value can meaningfully take.
    /// </summary>
    private static IEnumerable<(string Description, object Configuration)> Permutations(Type configurationType)
    {
        yield return ("default", Activator.CreateInstance(configurationType)!);

        foreach (PropertyInfo property in ConfigurationValues(configurationType))
        {
            foreach (var value in CandidateValues(configurationType, property))
            {
                var configuration = Activator.CreateInstance(configurationType)!;
                property.SetValue(configuration, value);

                yield return ($"{property.Name} = {value ?? "null"}", configuration);
            }

            // A configuration value can be an object with configuration values of its own, as the start node of the
            // multi node tree picker is, so permute one level down as well.
            if (HasNestedConfigurationValues(property.PropertyType) is false)
            {
                continue;
            }

            foreach (PropertyInfo nestedProperty in ConfigurationValues(property.PropertyType))
            {
                foreach (var value in CandidateValues(property.PropertyType, nestedProperty))
                {
                    var configuration = Activator.CreateInstance(configurationType)!;
                    var nestedConfiguration = Activator.CreateInstance(property.PropertyType)!;
                    nestedProperty.SetValue(nestedConfiguration, value);
                    property.SetValue(configuration, nestedConfiguration);

                    yield return ($"{property.Name}.{nestedProperty.Name} = {value ?? "null"}", configuration);
                }
            }
        }
    }

    private static IEnumerable<PropertyInfo> ConfigurationValues(Type configurationType)
        => configurationType
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(property => property.CanRead && property.CanWrite && property.GetIndexParameters().Length == 0);

    private static bool HasNestedConfigurationValues(Type type)
        => type.IsClass
           && type != typeof(string)
           && type.IsArray is false
           && type.GetConstructor(Type.EmptyTypes) is not null
           && ConfigurationValues(type).Any();

    /// <summary>
    /// Gets the values a single configuration value can meaningfully take. Values whose domain cannot be enumerated -
    /// collections and objects, for which there is no telling what a valid instance looks like - yield nothing.
    /// </summary>
    private static IEnumerable<object?> CandidateValues(Type configurationType, PropertyInfo property)
    {
        if (_constrainedDomains.TryGetValue(
                $"{configurationType.Name}.{property.Name}",
                out var constrainedValues))
        {
            return constrainedValues;
        }

        Type type = Nullable.GetUnderlyingType(property.PropertyType) ?? property.PropertyType;

        if (type == typeof(bool))
        {
            return [true, false];
        }

        if (type == typeof(int) || type == typeof(long))
        {
            return [Convert.ChangeType(0, type), Convert.ChangeType(1, type), Convert.ChangeType(2, type)];
        }

        if (type == typeof(decimal) || type == typeof(double) || type == typeof(float))
        {
            return [Convert.ChangeType(0, type), Convert.ChangeType(1, type)];
        }

        if (type == typeof(string))
        {
            return [string.Empty, "value"];
        }

        if (type == typeof(Guid))
        {
            return [Guid.Empty, Guid.Parse("11111111-1111-1111-1111-111111111111")];
        }

        if (type.IsEnum)
        {
            return Enum.GetValues(type).Cast<object?>();
        }

        return [];
    }

    /// <summary>
    /// Gets the string constants declared on a type, being the domain of a configuration value that is set from one
    /// of them.
    /// </summary>
    private static object?[] StringConstantsOf(Type type)
        => type
            .GetFields(BindingFlags.Public | BindingFlags.Static)
            .Where(field => field.IsLiteral && field.FieldType == typeof(string))
            .Select(field => field.GetRawConstantValue())
            .ToArray();
}
