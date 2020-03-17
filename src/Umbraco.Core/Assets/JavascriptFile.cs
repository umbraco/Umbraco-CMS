using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Assets
{
    /// <summary>
    /// Represents a JS asset file
    /// </summary>
    public class JavaScriptFile : AssetFile
    {
        public JavaScriptFile(string filePath)
            : base(AssetType.Javascript)
        {
            FilePath = filePath;
        }
    }
}
