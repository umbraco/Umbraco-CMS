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
    ///     Gets or sets the database type for the data type values.
    /// </summary>
    /// <remarks>
    ///     In most cases this is imposed by the property editor, but some editors
    ///     may support storing different types.
    /// </remarks>
    ValueStorageType DatabaseType { get; set; }

    /// <summary>
    /// Gets or sets the configuration object.
    /// </summary>
    /// <remarks>
    /// <para>The configuration object is serialized to Json and stored into the database.</para>
    /// <para>The serialized Json is deserialized by the property editor, which by default should
    /// return a Dictionary{string, object} but could return a typed object e.g. MyEditor.Configuration.</para>
    /// </remarks>
    object? Configuration { get; set; }

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

