using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Manifest;

// contentApps: [
//   {
//     name: 'App Name',        // required
//     alias: 'appAlias',       // required
//     weight: 0,               // optional, default is 0, use values between -99 and +99
//     icon: 'icon.app',        // required
//     view: 'path/view.htm',   // required
//     show: [                  // optional, default is always show
//       '-content/foo',        // hide for content type 'foo'
//       '+content/*',          // show for all other content types
//       '+media/*',            // show for all media types
//       '+role/admin'          // show for admin users. Role based permissions will override others.
//     ]
//   },
//   ...
// ]

/// <summary>
///     Represents a content app definition, parsed from a manifest.
/// </summary>
/// <remarks>Is used to create an actual <see cref="ManifestContentAppFactory" />.</remarks>
[DataContract(Name = "appdef", Namespace = "")]
public class ManifestContentAppDefinition
{
    private readonly string? _view;

    /// <summary>
    ///     Gets or sets the name of the content app.
    /// </summary>
    [DataMember(Name = "name")]
    public string? Name { get; set; }

    /// <summary>
    ///     Gets or sets the unique alias of the content app.
    /// </summary>
    /// <remarks>
    ///     <para>Must be a valid javascript identifier, ie no spaces etc.</para>
    /// </remarks>
    [DataMember(Name = "alias")]
    public string? Alias { get; set; }

    /// <summary>
    ///     Gets or sets the weight of the content app.
    /// </summary>
    [DataMember(Name = "weight")]
    public int Weight { get; set; }

    /// <summary>
    ///     Gets or sets the icon of the content app.
    /// </summary>
    /// <remarks>
    ///     <para>Must be a valid helveticons class name (see http://hlvticons.ch/).</para>
    /// </remarks>
    [DataMember(Name = "icon")]
    public string? Icon { get; set; }

    /// <summary>
    ///     Gets or sets the view for rendering the content app.
    /// </summary>
    [DataMember(Name = "view")]
    public string? View { get; set; }

    /// <summary>
    ///     Gets or sets the list of 'show' conditions for the content app.
    /// </summary>
    [DataMember(Name = "show")]
    public string[] Show { get; set; } = Array.Empty<string>();
}
