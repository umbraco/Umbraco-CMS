using System.Text.Json.Serialization;

namespace Umbraco.Cms.Core.Models.DeliveryApi;

[JsonDerivedType(typeof(RichTextRootElement))]
[JsonDerivedType(typeof(RichTextGenericElement))]
[JsonDerivedType(typeof(RichTextTextElement))]
public interface IRichTextElement
{
    string Tag { get; }
}
