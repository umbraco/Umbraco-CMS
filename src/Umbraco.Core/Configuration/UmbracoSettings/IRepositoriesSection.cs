using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{

    public interface IRepositoriesSection : IUmbracoConfigurationSection
    {
        IEnumerable<IRepository> Repositories { get; }
    }
}