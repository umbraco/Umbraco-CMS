// Copyright (c) Umbraco.
// See LICENSE for more details.

namespace Umbraco.Cms.Core.PropertyEditors;

/// <summary>
/// Provides a sortable string representation of property values for database sorting.
/// </summary>
/// <remarks>
/// <para>
/// Implement this interface on <see cref="Models.IDataValueEditor"/> implementations
/// when the stored value format does not naturally sort correctly (e.g., JSON-serialized dates).
/// </para>
/// <para>
/// The returned sortable value is stored in the database and used for sorting in collection views.
/// Values should be formatted to sort correctly when compared lexicographically (string comparison).
/// </para>
/// </remarks>
public interface IDataValueSortable
{
    /// <summary>
    /// Gets a sortable string representation of the property value.
    /// </summary>
    /// <param name="value">The stored property value.</param>
    /// <param name="dataTypeConfiguration">The data type configuration.</param>
    /// <returns>
    /// A string that sorts correctly when compared lexicographically, or <c>null</c> to use default sorting.
    /// The returned value should be no longer than 512 characters.
    /// </returns>
    string? GetSortableValue(object? value, object? dataTypeConfiguration);
}
