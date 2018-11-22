using NuGet;

namespace Umbraco.Core.Packaging
{
    internal interface IPackageContext
    {
        IPackageManager LocalPackageManager { get; }
        IPackageManager PublicPackageManager { get; }
        IPackagePathResolver LocalPathResolver { get; } 
    }
}