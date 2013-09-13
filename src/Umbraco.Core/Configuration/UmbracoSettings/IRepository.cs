using System;

namespace Umbraco.Core.Configuration.UmbracoSettings
{
    public interface IRepository
    {
        string Name { get; }
        Guid Id { get;  }
    }
}