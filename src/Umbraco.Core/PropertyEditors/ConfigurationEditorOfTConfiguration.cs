// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a data type configuration editor with a typed configuration.
/// </summary>
public abstract class ConfigurationEditor<TConfiguration> : ConfigurationEditor
    where TConfiguration : new()
{
    private readonly IEditorConfigurationParser _editorConfigurationParser;

    // Scheduled for removal in v12
    [Obsolete("Please use constructor that takes an IEditorConfigurationParser instead")]
    protected ConfigurationEditor(IIOHelper ioHelper)
        : this(ioHelper, StaticServiceProvider.Instance.GetRequiredService<IEditorConfigurationParser>())
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationEditor{TConfiguration}" /> class.
    /// </summary>
    protected ConfigurationEditor(IIOHelper ioHelper, IEditorConfigurationParser editorConfigurationParser)
        : base(DiscoverFields(ioHelper)) =>
        _editorConfigurationParser = editorConfigurationParser;

    /// <inheritdoc />
    public override object ToConfigurationObject(
        IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        try
        {
            if (configuration.Any() == false)
            {
                return new TConfiguration();
            }

            // TODO: quick fix for now (serialize to JSON, then deserialize to TConfiguration) - see if there is a better/more performant way (reverse of ObjectJsonExtensions.ToObjectDictionary)
            var json = configurationEditorJsonSerializer.Serialize(configuration);
            return configurationEditorJsonSerializer.Deserialize<TConfiguration>(json) ?? new TConfiguration();
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Failed to parse configuration \"{configuration}\" as \"{typeof(TConfiguration).Name}\" (see inner exception).",
                e);
        }
    }

    protected TConfiguration? AsConfigurationObject(IDictionary<string, object> configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer) =>
        ToConfigurationObject(configuration, configurationEditorJsonSerializer) is TConfiguration configurationObject
            ? configurationObject
            : default;

    /// <summary>
    ///     Discovers fields from configuration properties marked with the field attribute.
    /// </summary>
    private static List<ConfigurationField> DiscoverFields(IIOHelper ioHelper)
    {
        var fields = new List<ConfigurationField>();
        PropertyInfo[] properties = TypeHelper.CachedDiscoverableProperties(typeof(TConfiguration));

        foreach (PropertyInfo property in properties)
        {
            ConfigurationFieldAttribute? attribute = property.GetCustomAttribute<ConfigurationFieldAttribute>(false);
            if (attribute == null)
            {
                continue;
            }

            ConfigurationField field;

            var attributeView = ioHelper.ResolveRelativeOrVirtualUrl(attribute.View);

            // if the field does not have its own type, use the base type
            if (attribute.Type == null)
            {
                field = new ConfigurationField
                {
                    // if the key is empty then use the property name
                    Key = string.IsNullOrWhiteSpace(attribute.Key) ? property.Name : attribute.Key,
                    Name = attribute.Name,
                    PropertyName = property.Name,
                    PropertyType = property.PropertyType,
                    Description = attribute.Description,
                    HideLabel = attribute.HideLabel,
                    SortOrder = attribute.SortOrder,
                    View = attributeView,
                };

                fields.Add(field);
                continue;
            }

            // if the field has its own type, instantiate it
            try
            {
                field = (ConfigurationField)Activator.CreateInstance(attribute.Type)!;
            }
            catch (Exception ex)
            {
                throw new Exception(
                    $"Failed to create an instance of type \"{attribute.Type}\" for property \"{property.Name}\" of configuration \"{typeof(TConfiguration).Name}\" (see inner exception).",
                    ex);
            }

            // then add it, and overwrite values if they are assigned in the attribute
            fields.Add(field);

            field.PropertyName = property.Name;
            field.PropertyType = property.PropertyType;

            field.SortOrder = attribute.SortOrder;

            if (!string.IsNullOrWhiteSpace(attribute.Key))
            {
                field.Key = attribute.Key;
            }

            // if the key is still empty then use the property name
            if (string.IsNullOrWhiteSpace(field.Key))
            {
                field.Key = property.Name;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Name))
            {
                field.Name = attribute.Name;
            }

            if (!string.IsNullOrWhiteSpace(attribute.View))
            {
                field.View = attributeView;
            }

            if (!string.IsNullOrWhiteSpace(attribute.Description))
            {
                field.Description = attribute.Description;
            }

            if (attribute.HideLabelSettable.HasValue)
            {
                field.HideLabel = attribute.HideLabel;
            }
        }

        return fields.OrderBy(x => x.SortOrder).ToList();
    }
}
