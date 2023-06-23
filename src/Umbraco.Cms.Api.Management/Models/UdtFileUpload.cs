using System.Xml.Linq;

namespace Umbraco.Cms.Api.Management.Models;

public class UdtFileUpload
{
    public required XDocument Content { get; set; }

    public required string FileName { get; init; } = string.Empty;
}
