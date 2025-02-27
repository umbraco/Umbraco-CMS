using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a data type.
/// </summary>
public interface IDataType : IUmbracoEntity, IRememberBeingDirty
{
    /// <summary>
    ///     Gets or sets the property editor.
    /// </summary>
    IDataEditor? Editor { get; set; }

    /// <summary>
    ///     Gets the property editor alias.
    /// </summary>
    string EditorAlias { get; }

    /// <summary>
    ///     Gets the property editor UI alias.
    /// </summary>
    string? EditorUiAlias { get; set; }

    /// <summary>
    ///     Gets or sets the database type for the data type values.
    /// </summary>
    /// <remarks>
    ///     In most cases this is imposed by the property editor, but some editors
    ///     may support storing different types.
    /// </remarks>
    ValueStorageType DatabaseType { get; set; }

    /// <summary>
    /// Gets or sets the configuration data.
    /// </summary>
    IDictionary<string, object> ConfigurationData { get; set; }

    /// <summary>
    /// Gets an object representation of the configuration data.
    /// </summary>
    /// <remarks>
    /// The object type is dictated by the underlying <see cref="IConfigurationEditor"/> implementation of the <see cref="Editor"/>.
    /// </remarks>
    object? ConfigurationObject { get; }

    /// <summary>
    /// Creates a deep clone of the current entity with its identity/alias reset
    /// We have the default implementation here to avoid breaking changes for the user
    /// </summary>
    /// <returns></returns>
    IDataType DeepCloneWithResetIdentities()
    {
        var clone = (DataType)DeepClone();
        clone.Key = Guid.Empty;
        clone.ResetIdentity();
        return clone;
    }
}

