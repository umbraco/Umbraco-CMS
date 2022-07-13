using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Querying;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;

namespace Umbraco.Cms.Core.Services;

/// <summary>
///     Implements <see cref="IConsentService" />.
/// </summary>
internal class ConsentService : RepositoryService, IConsentService
{
    private readonly IConsentRepository _consentRepository;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ContentService" /> class.
    /// </summary>
    public ConsentService(ICoreScopeProvider provider, ILoggerFactory loggerFactory, IEventMessagesFactory eventMessagesFactory, IConsentRepository consentRepository)
        : base(provider, loggerFactory, eventMessagesFactory) =>
        _consentRepository = consentRepository;

    /// <inheritdoc />
    public IConsent RegisterConsent(string source, string context, string action, ConsentState state, string? comment = null)
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
            CreateDate = DateTime.Now,
            State = state,
            Comment = comment,
        };

        using (ICoreScope scope = ScopeProvider.CreateCoreScope())
        {
            _consentRepository.ClearCurrent(source, context, action);
            _consentRepository.Save(consent);
            scope.Complete();
        }

        return consent;
    }

    /// <inheritdoc />
    public IEnumerable<IConsent> LookupConsent(
        string? source = null,
        string? context = null,
        string? action = null,
        bool sourceStartsWith = false,
        bool contextStartsWith = false,
        bool actionStartsWith = false,
        bool includeHistory = false)
    {
        using (ScopeProvider.CreateCoreScope(autoComplete: true))
        {
            IQuery<IConsent> query = Query<IConsent>();

            if (string.IsNullOrWhiteSpace(source) == false)
            {
                query = sourceStartsWith
                    ? query.Where(x => x.Source!.StartsWith(source))
                    : query.Where(x => x.Source == source);
            }

            if (string.IsNullOrWhiteSpace(context) == false)
            {
                query = contextStartsWith
                    ? query.Where(x => x.Context!.StartsWith(context))
                    : query.Where(x => x.Context == context);
            }

            if (string.IsNullOrWhiteSpace(action) == false)
            {
                query = actionStartsWith
                    ? query.Where(x => x.Action!.StartsWith(action))
                    : query.Where(x => x.Action == action);
            }

            if (includeHistory == false)
            {
                query = query.Where(x => x.Current);
            }

            return _consentRepository.Get(query);
        }
    }
}
