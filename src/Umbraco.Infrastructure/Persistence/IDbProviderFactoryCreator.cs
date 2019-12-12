using System.Data.Common;
using StackExchange.Profiling.Internal;

namespace Umbraco.Core.Persistence
{
    public interface IDbProviderFactoryCreator
    {
        DbProviderFactory CreateFactory();
        DbProviderFactory CreateFactory(string providerName);
    }
}
