using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Umbraco.Core;
using Umbraco.Core.Assets;
using Umbraco.Core.PropertyEditors;
using Umbraco.Core.Runtime;
using Umbraco.Web.PropertyEditors;

namespace Umbraco.Web.JavaScript
{
    public abstract class AssetInitialization
    {
        private readonly IRuntimeMinifier _runtimeMinifier;
        private readonly PropertyEditorCollection _propertyEditorCollection;

        public AssetInitialization(IRuntimeMinifier runtimeMinifier, PropertyEditorCollection propertyEditorCollection)
        {
            _runtimeMinifier = runtimeMinifier;
            _propertyEditorCollection = propertyEditorCollection;
        }

        protected async Task<IEnumerable<string>> ScanPropertyEditorsAsync(AssetType assetType)
        {
            var attributes = _propertyEditorCollection
                    .SelectMany(x => x.GetType().GetCustomAttributes<PropertyEditorAssetAttribute>(false))
                    .Where(x => x.AssetType == assetType)
                    .Select(x => x.DependencyFile)
                    .ToList();

            return await _runtimeMinifier.GetAssetPathsAsync(assetType, attributes);
        }
    }
}
