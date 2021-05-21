using System.Collections.Generic;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.WebAssets
{
    public class CustomBackOfficeAssetsCollection : BuilderCollectionBase<IAssetFile>
    {
        public CustomBackOfficeAssetsCollection(IEnumerable<IAssetFile> items)
           : base(items)
        { }
    }
}
