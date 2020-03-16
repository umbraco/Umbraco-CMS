using Microsoft.Extensions.Configuration;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class TourSettings : ITourSettings
    {
        private readonly IConfiguration _configuration;
        public TourSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool EnableTours => _configuration.GetValue<bool?>("Umbraco:CMS:Tours:EnableTours") ?? true;

        public string Type { get; set; }
    }
}
