using System;

namespace Umbraco.Core.Packaging
{

    //TODO: We need to figure out how we want to do this
    // we have 2x repositories for saving created and installed packages
    // created packages can also be exported
    // maybe the below will work?

    public interface IInstalledPackagesRepository : IPackageDefinitionRepository
    {
    }
}
