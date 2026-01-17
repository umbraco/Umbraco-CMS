using System;
using System.Text.Json.Serialization;

namespace Umbraco.Cms.Api.Management.ViewModels.Template;

public class TemplateResponseModel : TemplateModelBase
{
    public Guid Id { get; set; }

    /// <summary>
    ///     Gets or sets the layout (parent template) reference.
    /// </summary>
    public ReferenceByIdModel? Layout { get; set; }

    /// <summary>
    ///     Gets or sets the master template reference.
    /// </summary>
    [JsonIgnore]
    [Obsolete("Use Layout instead. This will be removed in Umbraco 19.")]
    public ReferenceByIdModel? MasterTemplate { get => Layout; set => Layout = value; }
}
