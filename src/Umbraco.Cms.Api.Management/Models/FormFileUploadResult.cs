using System.Xml;

namespace Umbraco.Cms.Api.Management.Models;

public class FormFileUploadResult
{
    public bool CouldLoad { get; set; }

    public XmlDocument? XmlDocument { get; set; }

    public string? ErrorMessage { get; set; }

    public string? TemporaryPath { get; set; }
}
