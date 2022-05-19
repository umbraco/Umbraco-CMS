using System.Runtime.Serialization;
using Umbraco.Cms.Core.Strings;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a Template file that can have its content on disk.
/// </summary>
[Serializable]
[DataContract(IsReference = true)]
public class TemplateOnDisk : Template
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="TemplateOnDisk" /> class.
    /// </summary>
    /// <param name="name">The name of the template.</param>
    /// <param name="alias">The alias of the template.</param>
    /// <param name="shortStringHelper">The short string helper</param>
    public TemplateOnDisk(IShortStringHelper shortStringHelper, string name, string alias)
        : base(shortStringHelper, name, alias) =>
        IsOnDisk = true;

    /// <summary>
    ///     Gets or sets a value indicating whether the content is on disk already.
    /// </summary>
    public bool IsOnDisk { get; set; }

    /// <summary>
    ///     Gets or sets the content.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         Getting the content while the template is "on disk" throws,
    ///         the template must be saved before its content can be retrieved.
    ///     </para>
    ///     <para>
    ///         Setting the content means it is not "on disk" anymore, and the
    ///         template becomes (and behaves like) a normal template.
    ///     </para>
    /// </remarks>
    public override string? Content
    {
        get => IsOnDisk ? string.Empty : base.Content;
        set
        {
            base.Content = value;
            IsOnDisk = false;
        }
    }
}
