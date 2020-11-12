using System;

namespace Umbraco.Core.Configuration.Models
{
    public class LoggingSettings
    {
        public TimeSpan MaxLogAge { get; set; } = TimeSpan.FromHours(24);
    }
}
