// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
///     Represents a data type configuration editor with a typed configuration.
/// </summary>
public abstract class ConfigurationEditor<TConfiguration> : ConfigurationEditor
    where TConfiguration : new()
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ConfigurationEditor{TConfiguration}" /> class.
    /// </summary>
    protected ConfigurationEditor(IIOHelper ioHelper)
        : base(DiscoverFields(ioHelper))
    {
    }

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

    protected TConfiguration? AsConfigurationObject(
        IDictionary<string, object> configuration,
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

            // if the field does not have its own type, use the base type
            if (attribute.Type == null)
            {
                field = new ConfigurationField
                {
                    // if the key is empty then use the property name
                    Key = string.IsNullOrWhiteSpace(attribute.Key) ? property.Name : attribute.Key,
                    PropertyName = property.Name,
                    PropertyType = property.PropertyType,
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

            if (!string.IsNullOrWhiteSpace(attribute.Key))
            {
                field.Key = attribute.Key;
            }

            // if the key is still empty then use the property name
            if (string.IsNullOrWhiteSpace(field.Key))
            {
                field.Key = property.Name;
            }
        }

        return fields;
    }
}
