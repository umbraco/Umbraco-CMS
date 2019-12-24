using System;
using System.Collections.Generic;
using Umbraco.Core.Events;
using Umbraco.Core.Logging;
using Umbraco.Core.Models;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Scoping;

namespace Umbraco.Core.Services.Implement
{
    /// <summary>
    /// Implements <see cref="IConsentService"/>.
    /// </summary>
    internal class ConsentService : ScopeRepositoryService, IConsentService
    {
        private readonly IConsentRepository _consentRepository;

        /// <summary>
        /// Initializes a new instance of the <see cref="ContentService"/> class.
        /// </summary>
        public ConsentService(IScopeProvider provider, ILogger logger, IEventMessagesFactory eventMessagesFactory, IConsentRepository consentRepository)
            : base(provider, logger, eventMessagesFactory)
        {
            _consentRepository = consentRepository;
        }

        /// <inheritdoc />
        public IConsent RegisterConsent(string source, string context, string action, ConsentState state, string comment = null)
        {
            // prevent stupid states
            var v = 0;
            if ((state & ConsentState.Pending) > 0) v++;
            if ((state & ConsentState.Granted) > 0) v++;
            if ((state & ConsentState.Revoked) > 0) v++;
            if (v != 1)
                throw new ArgumentException("Invalid state.", nameof(state));

            var consent = new Consent
            {
                Current = true,
                Source = source,
                Context = context,
                Action = action,
                CreateDate = DateTime.Now,
                State = state,
                Comment = comment
            };

            using (var scope = ScopeProvider.CreateScope())
            {
                _consentRepository.ClearCurrent(source, context, action);
                _consentRepository.Save(consent);
                scope.Complete();
            }

            return consent;
        }

        /// <inheritdoc />
        public IEnumerable<IConsent> LookupConsent(string source = null, string context = null, string action = null,
            bool sourceStartsWith = false, bool contextStartsWith = false, bool actionStartsWith = false,
            bool includeHistory = false)
        {
            using (ScopeProvider.CreateScope(autoComplete: true))
            {
                var query = Query<IConsent>();

                if (string.IsNullOrWhiteSpace(source) == false)
                    query = sourceStartsWith ? query.Where(x => x.Source.StartsWith(source)) : query.Where(x => x.Source == source);
                if (string.IsNullOrWhiteSpace(context) == false)
                    query = contextStartsWith ? query.Where(x => x.Context.StartsWith(context)) : query.Where(x => x.Context == context);
                if (string.IsNullOrWhiteSpace(action) == false)
                    query = actionStartsWith ? query.Where(x => x.Action.StartsWith(action)) : query.Where(x => x.Action == action);
                if (includeHistory == false)
                    query = query.Where(x => x.Current);

                return _consentRepository.Get(query);
            }
        }
    }
}
