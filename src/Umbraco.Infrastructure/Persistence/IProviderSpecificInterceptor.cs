using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence;

public interface IProviderSpecificInterceptor : IInterceptor
{
    string ProviderName { get; }
}

public interface IProviderSpecificExecutingInterceptor : IProviderSpecificInterceptor, IExecutingInterceptor
{
}

public interface IProviderSpecificConnectionInterceptor : IProviderSpecificInterceptor, IConnectionInterceptor
{
}

public interface IProviderSpecificExceptionInterceptor : IProviderSpecificInterceptor, IExceptionInterceptor
{
}

public interface IProviderSpecificDataInterceptor : IProviderSpecificInterceptor, IDataInterceptor
{
}

public interface IProviderSpecificTransactionInterceptor : IProviderSpecificInterceptor, ITransactionInterceptor
{
}
