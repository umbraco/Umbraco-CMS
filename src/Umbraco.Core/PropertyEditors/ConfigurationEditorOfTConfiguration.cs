// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.IO;
using Umbraco.Cms.Core.Serialization;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Web.Common.DependencyInjection;
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
    public override IDictionary<string, object> DefaultConfiguration =>
        ToConfigurationEditor(DefaultConfigurationObject);

    /// <inheritdoc />
    public override object DefaultConfigurationObject => new TConfiguration();

    /// <inheritdoc />
    public override bool IsConfiguration(object obj)
        => obj is TConfiguration;

        /// <inheritdoc />
    public override object FromDatabase(
        string? configuration,
        IConfigurationEditorJsonSerializer configurationEditorJsonSerializer)
    {
        try
        {
            if (string.IsNullOrWhiteSpace(configuration))
            {
                return new TConfiguration();
            }

            return configurationEditorJsonSerializer.Deserialize<TConfiguration>(configuration)!;
        }
        catch (Exception e)
        {
            throw new InvalidOperationException(
                $"Failed to parse configuration \"{configuration}\" as \"{typeof(TConfiguration).Name}\" (see inner exception).",
                e);
        }
    }

    /// <inheritdoc />
    public sealed override object? FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        object? configuration) => FromConfigurationEditor(editorValues, (TConfiguration?)configuration);

    /// <summary>
    ///     Converts the configuration posted by the editor.
    /// </summary>
    /// <param name="editorValues">The configuration object posted by the editor.</param>
    /// <param name="configuration">The current configuration object.</param>
    public virtual TConfiguration? FromConfigurationEditor(
        IDictionary<string, object?>? editorValues,
        TConfiguration? configuration) =>
        _editorConfigurationParser.ParseFromConfigurationEditor<TConfiguration>(editorValues, Fields);

    /// <inheritdoc />
    public sealed override IDictionary<string, object> ToConfigurationEditor(object? configuration) =>
        ToConfigurationEditor((TConfiguration?)configuration);

    /// <summary>
    ///     Converts configuration values to values for the editor.
    /// </summary>
    /// <param name="configuration">The configuration.</param>
    public virtual Dictionary<string, object> ToConfigurationEditor(TConfiguration? configuration) =>
        _editorConfigurationParser.ParseToConfigurationEditor(configuration);

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

        return fields;
    }
}
