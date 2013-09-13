using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRepositories
    {
        IEnumerable<IRepository> Repositories { get; }
    }
}