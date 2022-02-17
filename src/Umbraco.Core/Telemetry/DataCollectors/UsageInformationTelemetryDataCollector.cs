using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Cms.Core.Models.Membership;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Telemetry.Models;

namespace Umbraco.Cms.Core.Telemetry.DataCollectors
{
    /// <summary>
    /// Collects usage information telemetry data.
    /// </summary>
    /// <seealso cref="Umbraco.Cms.Core.Telemetry.ITelemetryDataCollector" />
    internal class UsageInformationTelemetryDataCollector : ITelemetryDataCollector
    {
        private readonly IRuntimeState _runtimeState;
        private readonly IContentService _contentService;
        private readonly IDomainService _domainService;
        private readonly IMediaService _mediaService;
        private readonly IMemberService _memberService;
        private readonly ILocalizationService _localizationService;
        private readonly IDataTypeService _dataTypeService;
        private readonly IMacroService _macroService;

        private static readonly IEnumerable<TelemetryData> s_data_run = new[]
        {
            TelemetryData.RuntimeLevel,
            TelemetryData.ContentCount,
            TelemetryData.DomainCount,
            TelemetryData.MediaCount,
            TelemetryData.MemberCount,
            TelemetryData.Languages,
            TelemetryData.PropertyEditors,
            TelemetryData.MacroCount
        };

        private static readonly IEnumerable<TelemetryData> s_data = new[]
        {
            TelemetryData.RuntimeLevel
        };

        /// <summary>
        /// Initializes a new instance of the <see cref="UsageInformationTelemetryDataCollector" /> class.
        /// </summary>
        public UsageInformationTelemetryDataCollector(
            IRuntimeState runtimeState,
            IContentService contentService,
            IDomainService domainService,
            IMediaService mediaService,
            IMemberService memberService,
            ILocalizationService localizationService,
            IDataTypeService dataTypeService,
            IMacroService macroService)
        {
            _runtimeState = runtimeState;
            _contentService = contentService;
            _domainService = domainService;
            _mediaService = mediaService;
            _memberService = memberService;
            _localizationService = localizationService;
            _dataTypeService = dataTypeService;
            _macroService = macroService;
        }

        /// <inheritdoc/>
        public IEnumerable<TelemetryData> Data => _runtimeState.Level switch
        {
            RuntimeLevel.Run => s_data_run,
            _ => s_data
        };

        /// <inheritdoc/>
        public object Collect(TelemetryData telemetryData) => telemetryData switch
        {
            TelemetryData.RuntimeLevel => _runtimeState.Level,
            TelemetryData.ContentCount => GetContentCount(),
            TelemetryData.DomainCount => GetDomainCount(),
            TelemetryData.MediaCount => GetMediaCount(),
            TelemetryData.MemberCount => GetMemberCount(),
            TelemetryData.Languages => GetLanguages(),
            TelemetryData.PropertyEditors => GetPropertyEditors(),
            TelemetryData.MacroCount => GetMacroCount(),
            _ => throw new NotSupportedException()
        };

        private ContentCount GetContentCount() => new()
        {
            Count = _contentService.Count(),
            Published = _contentService.CountPublished()
        };

        private DomainCount GetDomainCount()
        {
            var domains = _domainService.GetAll(true);

            return new()
            {
                Count = domains.Count(),
                Wildcards = domains.Count(x => x.IsWildcard)
            };
        }

        private MediaCount GetMediaCount() => new()
        {
            Count = _mediaService.Count(),
            NotTrashed = _mediaService.CountNotTrashed()
        };

        private MemberCount GetMemberCount() => new()
        {
            Count = _memberService.GetCount(MemberCountType.All),
            Approved = _memberService.GetCount(MemberCountType.Approved),
            LockedOut = _memberService.GetCount(MemberCountType.LockedOut),
        };

        private IDictionary<string, Language> GetLanguages()
            => _localizationService.GetAllLanguages().ToDictionary(x => x.IsoCode, x => new Language
            {
                IsDefault = x.IsDefault,
                IsMandatory = x.IsMandatory,
                HasFallback = x.FallbackLanguageId.HasValue
            });

        private IDictionary<string, int> GetPropertyEditors()
            => _dataTypeService.GetAll().GroupBy(x => x.EditorAlias).ToDictionary(x => x.Key, x => x.Count());

        private MacroCount GetMacroCount()
        {
            var macros = _macroService.GetAll();

            return new()
            {
                Count = macros.Count(),
                UseInEditor = macros.Count(x => x.UseInEditor)
            };
        }

        private class ContentCount
        {
            public int Count { get; set; }
            public int Published { get; set; }
        }

        private class DomainCount
        {
            public int Count { get; set; }
            public int Wildcards { get; set; }
        }

        private class MediaCount
        {
            public int Count { get; set; }
            public int NotTrashed { get; set; }
        }

        private class MemberCount
        {
            public int Count { get; set; }
            public int Approved { get; set; }
            public int LockedOut { get; set; }
        }

        private class Language
        {
            public bool IsDefault { get; set; }
            public bool IsMandatory { get; set; }
            public bool HasFallback { get; set; }
        }

        private class MacroCount
        {
            public int Count { get; set; }
            public int UseInEditor { get; set; }
        }
    }
}
