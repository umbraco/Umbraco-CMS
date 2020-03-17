using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Assets
{
    /// <summary>
    /// Represents a JS asset file
    /// </summary>
    public class JavascriptFile : AssetFile
    {
        public JavascriptFile(string filePath)
            : base(AssetType.Javascript)
        {
            FilePath = filePath;
        }
    }
}
