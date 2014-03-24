using Umbraco.Core.Services;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageBuilding
    {
    }

    internal class PackageBuilding : IPackageBuilding
    {
        private readonly PackagingService _packagingService;

        public PackageBuilding(PackagingService packagingService)
        {
            _packagingService = packagingService;
        }
    }
}