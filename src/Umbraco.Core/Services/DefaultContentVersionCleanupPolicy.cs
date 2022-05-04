using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Persistence.Repositories;
using Umbraco.Cms.Core.Scoping;
using ContentVersionCleanupPolicySettings = Umbraco.Cms.Core.Models.ContentVersionCleanupPolicySettings;

namespace Umbraco.Cms.Core.Services
{
    public class DefaultContentVersionCleanupPolicy : IContentVersionCleanupPolicy
    {
        private readonly IOptions<ContentSettings> _contentSettings;
        private readonly ICoreScopeProvider _scopeProvider;
        private readonly IDocumentVersionRepository _documentVersionRepository;

        public DefaultContentVersionCleanupPolicy(IOptions<ContentSettings> contentSettings, ICoreScopeProvider scopeProvider, IDocumentVersionRepository documentVersionRepository)
        {
            _contentSettings = contentSettings ?? throw new ArgumentNullException(nameof(contentSettings));
            _scopeProvider = scopeProvider ?? throw new ArgumentNullException(nameof(scopeProvider));
            _documentVersionRepository = documentVersionRepository ?? throw new ArgumentNullException(nameof(documentVersionRepository));
        }

        public IEnumerable<ContentVersionMeta> Apply(DateTime asAtDate, IEnumerable<ContentVersionMeta> items)
        {
            // Note: Not checking global enable flag, that's handled in the scheduled job.
            // If this method is called and policy is globally disabled someone has chosen to run in code.

            var globalPolicy = _contentSettings.Value.ContentVersionCleanupPolicy;

            var theRest = new List<ContentVersionMeta>();

            using(_scopeProvider.CreateCoreScope(autoComplete: true))
            {
                var policyOverrides = _documentVersionRepository.GetCleanupPolicies()?
                    .ToDictionary(x => x.ContentTypeId);

                foreach (var version in items)
                {
                    var age = asAtDate - version.VersionDate;

                    var overrides = GetOverridePolicy(version, policyOverrides);

                    var keepAll = overrides?.KeepAllVersionsNewerThanDays ?? globalPolicy.KeepAllVersionsNewerThanDays!;
                    var keepLatest = overrides?.KeepLatestVersionPerDayForDays ?? globalPolicy.KeepLatestVersionPerDayForDays;
                    var preventCleanup = overrides?.PreventCleanup ?? false;

                    if (preventCleanup)
                    {
                        continue;
                    }

                    if (age.TotalDays <= keepAll)
                    {
                        continue;
                    }

                    if (age.TotalDays > keepLatest)
                    {

                        yield return version;
                        continue;
                    }

                    theRest.Add(version);
                }

                var grouped = theRest.GroupBy(x => new
                {
                    x.ContentId,
                    x.VersionDate.Date
                });

                foreach (var group in grouped)
                {
                    foreach (var version in group.OrderByDescending(x => x.VersionId).Skip(1))
                    {
                        yield return version;
                    }
                }
            }
        }

        private ContentVersionCleanupPolicySettings? GetOverridePolicy(
            ContentVersionMeta version,
            IDictionary<int, ContentVersionCleanupPolicySettings>? overrides)
        {
            if (overrides is null)
            {
                return null;
            }

            _ = overrides.TryGetValue(version.ContentTypeId, out var value);

            return value;
        }
    }
}
