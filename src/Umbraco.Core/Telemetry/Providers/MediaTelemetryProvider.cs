using System.Collections.Generic;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Services;

namespace Umbraco.Cms.Core.Telemetry.Providers
{
    public class MediaTelemetryProvider : IDetailedTelemetryProvider
    {
        private readonly IMediaService _mediaService;

        public MediaTelemetryProvider(IMediaService mediaService)
        {
            _mediaService = mediaService;
        }

        public IEnumerable<UsageInformation> GetInformation()
        {
            var result = new List<UsageInformation>();
            int mediaItems = _mediaService.CountNotTrashed();
            result.Add(new("Media Items", mediaItems));
            return result;
        }
    }
}
