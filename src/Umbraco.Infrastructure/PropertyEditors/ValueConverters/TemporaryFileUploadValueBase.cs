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

    protected bool Equals(TemporaryFileUploadValueBase other) => Nullable.Equals(TemporaryFileId, other.TemporaryFileId) && Src == other.Src;

    public override bool Equals(object? obj)
    {
        if (ReferenceEquals(null, obj))
        {
            return false;
        }

        if (ReferenceEquals(this, obj))
        {
            return true;
        }

        if (obj.GetType() != this.GetType())
        {
            return false;
        }

        return Equals((TemporaryFileUploadValueBase)obj);
    }

    public override int GetHashCode() => HashCode.Combine(TemporaryFileId, Src);
}
