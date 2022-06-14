using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.WebAssets;

public class CustomBackOfficeAssetsCollection : BuilderCollectionBase<IAssetFile>
{
    public CustomBackOfficeAssetsCollection(Func<IEnumerable<IAssetFile>> items)
        : base(items)
    {
    }
}
