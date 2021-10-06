using System;
using System.Collections.Generic;
using Microsoft.Extensions.DependencyInjection;
using Umbraco.Cms.Core.Composing;

namespace Umbraco.Cms.Core.WebAssets
{
    public class CustomBackOfficeAssetsCollectionBuilder : ICollectionBuilder<CustomBackOfficeAssetsCollection, IAssetFile>
    {
        private readonly List<IAssetFile> _files = new List<IAssetFile>();

        public CustomBackOfficeAssetsCollection CreateCollection(IServiceProvider factory) => new CustomBackOfficeAssetsCollection(() => _files);

        public void RegisterWith(IServiceCollection services) => services.Add(new ServiceDescriptor(typeof(CustomBackOfficeAssetsCollection), CreateCollection, ServiceLifetime.Singleton));

        public void AddAssetFile(IAssetFile file)
        {
            if (file is null) throw new ArgumentNullException(nameof(file));
            _files.Add(file);
        }
    }
}
