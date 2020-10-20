using System.Collections.Generic;
using Umbraco.Core.Configuration.UmbracoSettings;

namespace Umbraco.Core.Configuration.Models
{
    public class RequestHandlerSettings
    {
        internal static readonly CharItem[] DefaultCharCollection =
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

        public bool AddTrailingSlash { get; set; } = true;

        public string ConvertUrlsToAscii { get; set; } = "try";

        public bool ShouldConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("true");

        public bool ShouldTryConvertUrlsToAscii => ConvertUrlsToAscii.InvariantEquals("try");

        //We need to special handle ":", as this character is special in keys

        // TODO: implement from configuration

        //var collection = _configuration.GetSection(Prefix + "CharCollection").GetChildren()
        //    .Select(x => new CharItem()
        //    {
        //        Char = x.GetValue<string>("Char"),
        //        Replacement = x.GetValue<string>("Replacement"),
        //    }).ToArray();

        //if (collection.Any() || _configuration.GetSection("Prefix").GetChildren().Any(x =>
        //    x.Key.Equals("CharCollection", StringComparison.OrdinalIgnoreCase)))
        //{
        //    return collection;
        //}

        // return DefaultCharCollection;
        public IEnumerable<IChar> CharCollection { get; set; } = DefaultCharCollection;

        public class CharItem : IChar
        {
            public string Char { get; set; }

            public string Replacement { get; set; }
        }
    }
}
