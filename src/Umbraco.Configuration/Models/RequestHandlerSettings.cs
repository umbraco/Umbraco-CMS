using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class RequestHandlerSettings : IRequestHandlerSettings
    {
        private readonly IConfiguration _configuration;
        public RequestHandlerSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool AddTrailingSlash   => _configuration.GetValue<bool?>("Umbraco:CMS:RequestHandler:AddTrailingSlash") ?? true;
        public bool ConvertUrlsToAscii   => _configuration.GetValue<string>("Umbraco:CMS:RequestHandler:ConvertUrlsToAscii").InvariantEquals("true");
        public bool TryConvertUrlsToAscii   => _configuration.GetValue<string>("Umbraco:CMS:RequestHandler:ConvertUrlsToAscii").InvariantEquals("try");


        //We need to special handle ":", as this character is special in keys
        public IEnumerable<IChar> CharCollection => _configuration.GetSection("Umbraco:CMS:RequestHandler:CharCollection")
            .GetChildren()
            .Select(kvp => new CharItem()
            {
                Char = kvp.Key,
                Replacement = kvp.Value
            }).Union(new []
            {
                new CharItem(){Char = ":", Replacement = string.Empty},
            });

        private class CharItem : IChar
        {

            public string Char { get; set; }
            public string Replacement { get; set; }
        }
    }
}
