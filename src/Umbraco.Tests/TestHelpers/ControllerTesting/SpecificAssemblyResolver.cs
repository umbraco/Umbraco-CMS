using System.Collections.Generic;
using System.Reflection;
using System.Web.Http.Dispatcher;

namespace Umbraco.Tests.TestHelpers.ControllerTesting
{
    public class SpecificAssemblyResolver : IAssembliesResolver
    {
        private readonly Assembly[] _assemblies;

        public SpecificAssemblyResolver(Assembly[] assemblies)
        {
            _assemblies = assemblies;
        }

        public ICollection<Assembly> GetAssemblies()
        {
            return _assemblies;
        }
    }
}
