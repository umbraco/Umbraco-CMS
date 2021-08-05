using System.Collections.Generic;
using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services
{
    public interface ITestService : IService
    {
        IEnumerable<ITest> GetAll();
    }
}
