using System;

namespace Umbraco.Cms.Core.Services
{
    public interface INodeCountService
    {
        public int GetNodeCount(Guid nodeType);
    }
}
