using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRepository
    {
        string Name { get; }
        Guid Id { get;  }
        string RepositoryUrl { get; }
        string WebServiceUrl { get; }
        bool HasCustomWebServiceUrl { get; }
    }
}