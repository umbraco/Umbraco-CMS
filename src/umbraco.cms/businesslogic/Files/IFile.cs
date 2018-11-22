using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.Files
{
    [Obsolete("This is no longer used ane will be removed from the codebase in the future")]
    public interface IFile
    {
        string Filename { get; }
        string Extension { get; }
        [Obsolete("LocalName is obsolete, please use URL instead", false)]
        string LocalName { get; }
        string Path { get; }
        string Url { get; }
        bool SupportsResizing { get; }
        string GetFriendlyName();
        System.Tuple<int, int> GetDimensions();
        string Resize(int width, int height);
        string Resize(int maxWidthHeight, string fileNameAddition);
    }
}
