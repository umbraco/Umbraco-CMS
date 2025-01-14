using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.PropertyEditors.ValueConverters;

public abstract class TemporaryFileUploadValueBase
{
    /// <summary>
    /// Gets or sets the temporary file identifier that will replace an an existing <see cref="Src"/> value.
    /// </summary>
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public Guid? TemporaryFileId { get; set; }

    /// <summary>
    ///     Gets or sets the value source image.
    /// </summary>
    public string? Src { get; set; } = string.Empty;
}
