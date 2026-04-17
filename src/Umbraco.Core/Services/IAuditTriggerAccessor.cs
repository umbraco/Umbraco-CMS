using Umbraco.Cms.Core.Models;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Provides access to the ambient <see cref="AuditTrigger" /> for the current scope.
/// </summary>
/// <remarks>
///     <para>
///         The trigger is stored on the ambient <see cref="Scoping.IScopeContext" /> using
///         <see cref="Scoping.IScopeContext.Enlist{T}" />. A scope must be active before calling <see cref="Set" />.
///     </para>
///     <para>
///         First-writer-wins: if a trigger has already been set on the current scope context,
///         subsequent calls to <see cref="Set" /> are no-ops. This ensures that an outer caller
///         (e.g. a package or background job) cannot be overwritten by inner code.
///     </para>
/// </remarks>
public interface IAuditTriggerAccessor
{
    /// <summary>
    ///     Gets the current ambient <see cref="AuditTrigger" />, or <c>null</c> if none has been set.
    /// </summary>
    AuditTrigger? Current { get; }

    /// <summary>
    ///     Sets the ambient <see cref="AuditTrigger" /> for the current scope.
    ///     This is a no-op if a trigger has already been set (first-writer-wins).
    /// </summary>
    /// <param name="trigger">The trigger to set.</param>
    void Set(AuditTrigger trigger);
}
