using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     A simple representation of an Umbraco member property
/// </summary>
public class MemberPropertyModel
{
    [Required]
    public string Alias { get; set; } = null!;

    // NOTE: This has to be a string currently, if it is an object it will bind as an array which we don't want.
    // If we want to have this as an 'object' with a true type on it, we have to create a custom model binder
    // for an UmbracoProperty and then bind with the correct type based on the property type for this alias. This
    // would be a bit long winded and perhaps unnecessary. The reason is because it is always posted as a string anyways
    // and when we set this value on the property object that gets sent to the database we do a TryConvertTo to the
    // real type anyways.
    [DataType(System.ComponentModel.DataAnnotations.DataType.Text)]
    public string? Value { get; set; }

    [ReadOnly(true)]
    public string? Name { get; set; }

    // TODO: Perhaps one day we'll ship with our own EditorTempates but for now developers can just render their own inside the view

    ///// <summary>
    ///// This can dynamically be set to a custom template name to change
    ///// the editor type for this property
    ///// </summary>
    // [ReadOnly(true)]
    // public string EditorTemplate { get; set; }
}
