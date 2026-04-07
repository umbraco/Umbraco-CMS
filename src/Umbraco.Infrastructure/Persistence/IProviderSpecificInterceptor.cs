using NPoco;

namespace Umbraco.Cms.Infrastructure.Persistence;

/// <summary>
/// Provides methods for intercepting and customizing behavior specific to a database provider.
/// </summary>
public interface IProviderSpecificInterceptor : IInterceptor
{
    /// <summary>
    /// Gets the name of the database provider that this interceptor targets.
    /// </summary>
    string ProviderName { get; }
}

/// <summary>
/// Represents an interceptor that allows execution of provider-specific logic during persistence operations.
/// </summary>
public interface IProviderSpecificExecutingInterceptor : IProviderSpecificInterceptor, IExecutingInterceptor
{
}

/// <summary>
/// Represents an interceptor that allows customization or monitoring of database connection operations specific to a particular database provider.
/// </summary>
public interface IProviderSpecificConnectionInterceptor : IProviderSpecificInterceptor, IConnectionInterceptor
{
}

/// <summary>
/// Represents an interceptor that handles exceptions specific to a particular database provider.
/// Implementations can use this to translate or process provider-specific exceptions.
/// </summary>
public interface IProviderSpecificExceptionInterceptor : IProviderSpecificInterceptor, IExceptionInterceptor
{
}

/// <summary>
/// Provides methods for intercepting and modifying data operations that are specific to a particular database provider.
/// Implementations can customize behavior based on the underlying database system.
/// </summary>
public interface IProviderSpecificDataInterceptor : IProviderSpecificInterceptor, IDataInterceptor
{
}

/// <summary>
/// Represents an interceptor that enables customization of transaction behaviors specific to a database provider.
/// </summary>
public interface IProviderSpecificTransactionInterceptor : IProviderSpecificInterceptor, ITransactionInterceptor
{
}
