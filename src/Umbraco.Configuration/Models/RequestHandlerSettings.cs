using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Configuration;
using Umbraco.Core;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Configuration.Models
{
    internal class RequestHandlerSettings : IRequestHandlerSettings
    {
        private const string Prefix = Constants.Configuration.ConfigPrefix + "RequestHandler:";
        private static readonly CharItem[] DefaultCharCollection =
        {
            new CharItem { Char = " ", Replacement = "-" },
            new CharItem { Char = "\"", Replacement = "" },
            new CharItem { Char = "'", Replacement = "" },
            new CharItem { Char = "%", Replacement = "" },
            new CharItem { Char = ".", Replacement = "" },
            new CharItem { Char = ";", Replacement = "" },
            new CharItem { Char = "/", Replacement = "" },
            new CharItem { Char = "\\", Replacement = "" },
            new CharItem { Char = ":", Replacement = "" },
            new CharItem { Char = "#", Replacement = "" },
            new CharItem { Char = "+", Replacement = "plus" },
            new CharItem { Char = "*", Replacement = "star" },
            new CharItem { Char = "&", Replacement = "" },
            new CharItem { Char = "?", Replacement = "" },
            new CharItem { Char = "æ", Replacement = "ae" },
            new CharItem { Char = "ä", Replacement = "ae" },
            new CharItem { Char = "ø", Replacement = "oe" },
            new CharItem { Char = "ö", Replacement = "oe" },
            new CharItem { Char = "å", Replacement = "aa" },
            new CharItem { Char = "ü", Replacement = "ue" },
            new CharItem { Char = "ß", Replacement = "ss" },
            new CharItem { Char = "|", Replacement = "-" },
            new CharItem { Char = "<", Replacement = "" },
            new CharItem { Char = ">", Replacement = "" }
        };

        private readonly IConfiguration _configuration;

        public RequestHandlerSettings(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public bool AddTrailingSlash =>
            _configuration.GetValue(Prefix+"AddTrailingSlash", true);

        public bool ConvertUrlsToAscii => _configuration
            .GetValue<string>(Prefix+"ConvertUrlsToAscii").InvariantEquals("true");

        public bool TryConvertUrlsToAscii => _configuration
            .GetValue<string>(Prefix+"ConvertUrlsToAscii").InvariantEquals("try");


        //We need to special handle ":", as this character is special in keys
        public IEnumerable<IChar> CharCollection
        {
            get
            {
                var collection = _configuration.GetSection(Prefix + "CharCollection").GetChildren()
                    .Select(x => new CharItem()
                    {
                        Char = x.GetValue<string>("Char"),
                        Replacement = x.GetValue<string>("Replacement"),
                    }).ToArray();

                if (collection.Any() || _configuration.GetSection("Prefix").GetChildren().Any(x =>
                    x.Key.Equals("CharCollection", StringComparison.OrdinalIgnoreCase)))
                {
                    return collection;
                }

                return DefaultCharCollection;
            }
        }


        public class CharItem : IChar
        {
            public string Char { get; set; }
            public string Replacement { get; set; }
        }
    }
}
