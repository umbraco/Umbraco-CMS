using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace umbraco.cms.businesslogic.Files
{
    public interface IFile
    {
        string Filename { get; }
        string Extension { get; }
        string LocalName { get; }
        bool SupportsResizing { get; }
        string GetFriendlyName();
        System.Tuple<int, int> GetDimensions();
        string Resize(int width, int height);
        string Resize(int maxWidthHeight, string fileNameAddition);
    }
}
