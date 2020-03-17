using System;
using System.Collections.Generic;
using System.Text;

namespace Umbraco.Core.Assets
{
    /// <summary>
    /// Represents a CSS asset file
    /// </summary>
    public class CssFile : AssetFile
    {
        public CssFile(string filePath)
            : base(AssetType.Css)
        {
            FilePath = filePath;
        }
    }
}
