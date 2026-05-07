using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using Umbraco.Cms.Core.Scoping.EFCore;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IConsentService" />.
/// </summary>
internal sealed class ConsentService : AsyncRepositoryService, IConsentService
{
    private readonly IConsentRepository _consentRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentService" /> class.
    /// </summary>
    public ConsentService(
        IScopeProvider provider,
        ILoggerFactory loggerFactory,
        IEventMessagesFactory eventMessagesFactory,
        IConsentRepository consentRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _consentRepository = consentRepository;

    /// <inheritdoc />
    public async Task<IConsent> RegisterConsentAsync(string source, string context, string action, ConsentState state, string? comment = null)
    {
        // prevent stupid states
        var v = 0;
        if ((state & ConsentState.Pending) > 0)
        {
            v++;
        }

        if ((state & ConsentState.Granted) > 0)
        {
            v++;
        }

        if ((state & ConsentState.Revoked) > 0)
        {
            v++;
        }

        if (v != 1)
        {
            throw new ArgumentException("Invalid state.", nameof(state));
        }

        var consent = new Consent
        {
            Current = true,
            Source = source,
            Context = context,
            Action = action,
            CreateDate = DateTime.UtcNow,
            State = state,
            Comment = comment,
        };

        using ICoreScope scope = ScopeProvider.CreateScope();

        await _consentRepository.ClearCurrentAsync(source, context, action);
        await _consentRepository.SaveAsync(consent, CancellationToken.None);

        scope.Complete();

        return consent;
    }

    /// <inheritdoc />
    public async Task<IEnumerable<IConsent>> LookupConsentAsync(
        string? source = null,
        string? context = null,
        string? action = null,
        bool sourceStartsWith = false,
        bool contextStartsWith = false,
        bool actionStartsWith = false,
        bool includeHistory = false)
    {
        using ICoreScope scope = ScopeProvider.CreateScope();

        IEnumerable<IConsent> result = await _consentRepository.LookupAsync(source, context, action, sourceStartsWith, contextStartsWith, actionStartsWith, includeHistory);

        scope.Complete();
        return result;
    }
}
