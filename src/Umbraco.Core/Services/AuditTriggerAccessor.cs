using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Default implementation of <see cref="IAuditTriggerAccessor"/> backed by <see cref="ICoreScopeProvider.Context"/>.
/// </summary>
public sealed class AuditTriggerAccessor : IAuditTriggerAccessor
{
    private const string EnlistmentKey = Constants.Audit.TriggerEnlistmentKey;
    private readonly ICoreScopeProvider _coreScopeProvider;

    /// <summary>
    ///     Initializes a new instance of the <see cref="AuditTriggerAccessor"/> class.
    /// </summary>
    /// <param name="coreScopeProvider">The core scope provider used to access the ambient scope context.</param>
    public AuditTriggerAccessor(ICoreScopeProvider coreScopeProvider)
        => _coreScopeProvider = coreScopeProvider;

    /// <inheritdoc />
    public AuditTrigger? Current
        => _coreScopeProvider.Context?.GetEnlisted<AuditTrigger>(EnlistmentKey);

    /// <inheritdoc />
    public void Set(AuditTrigger trigger)
        => _coreScopeProvider.Context?.Enlist(EnlistmentKey, () => trigger);
}
