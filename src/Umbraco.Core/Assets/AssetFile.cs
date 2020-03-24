using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Umbraco.Core.Assets
{
    /// <summary>
    /// Represents a dependency file
    /// </summary>
    [DebuggerDisplay("Type: {DependencyType}, File: {FilePath}")]
    public class AssetFile : IAssetFile
    {
        #region IAssetFile Members

        public string FilePath { get; set; }
        public AssetType DependencyType { get; }
        public int Priority { get; set; }
        public int Group { get; set; }
        public string PathNameAlias { get; set; }
        public string ForceProvider { get; set; }
        public string Bundle { get; }

        /// <summary>
        /// Used to store additional attributes in the HTML markup for the item
        /// </summary>
        /// <remarks>
        /// Mostly used for CSS Media, but could be for anything
        /// </remarks>
        public IDictionary<string, string> HtmlAttributes { get; }

        #endregion

        public AssetFile(AssetType type, string bundleName = "unspecfed")
        {
            DependencyType = type;
            HtmlAttributes = new Dictionary<string, string>();
            // Set to 100 for the case when a developer doesn't specify a priority it will come after all other dependencies that 
            // have unless the priority is explicitly set above 100.
            Priority = 100;
            //Unless a group is specified, all dependencies will go into the same, default, group.
            Group = 100;
            Bundle = bundleName;
        }
    }
}
