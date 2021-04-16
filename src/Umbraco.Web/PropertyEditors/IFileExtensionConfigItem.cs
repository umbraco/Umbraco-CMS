using Newtonsoft.Json;

namespace Umbraco.Web.PropertyEditors
{
    public interface IFileExtensionConfigItem
    {
        int Id { get; set; }

        string Value { get; set; }
    }
}
