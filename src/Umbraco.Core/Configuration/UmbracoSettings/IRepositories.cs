using System.Collections.Generic;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    //TODO: Where do we put the 'package server' setting?

    public interface IRepositories
    {
        IEnumerable<IRepository> Repositories { get; }
    }
}