using Moq;
using Umbraco.Core;
using Umbraco.Core.Composing;
using Umbraco.Core.WebAssets;

namespace Umbraco.Tests.Integration.Testing
{
    /// <summary>
    /// This is used to replace certain services that are normally registered from our Core / Infrastructure that
    /// we do not want active within integration tests
    /// </summary>
    /// <remarks>
    /// This is a IUserComposer so that it runs after all core composers
    /// </remarks>
    [RuntimeLevel(MinLevel = RuntimeLevel.Boot)]
    public class IntegrationTestComposer : IUserComposer
    {
        public void Compose(Composition composition)
        {
            composition.RegisterUnique<IRuntimeMinifier>(factory => Mock.Of<IRuntimeMinifier>());
        }
    }
}
