using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageBuilding
    {
    }

    internal class PackageBuilding : IPackageBuilding
    {
        private readonly IPackagingService _packagingService;

        public PackageBuilding(IPackagingService packagingService)
        {
            _packagingService = packagingService;
        }
    }
}