namespace Umbraco.Cms.Core.IO;

public interface IDefaultViewContentProvider
{
    string GetDefaultFileContent(string? layoutPageAlias = null, string? modelClassName = null, string? modelNamespace = null, string? modelNamespaceAlias = null);
}
