using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.IO;

public interface IViewHelper
{
    bool ViewExists(ITemplate t);

    string GetFileContents(ITemplate t);

    string CreateView(ITemplate t, bool overWrite = false);

    string? UpdateViewFile(ITemplate t, string? currentAlias = null);

    string ViewPath(string alias);
}
