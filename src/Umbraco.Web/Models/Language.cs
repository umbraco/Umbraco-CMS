using System.Globalization;

namespace Umbraco.Web.Models
{
    public class Language
    {
        public int Id { get; set; }
        public string IsoCode { get; set; }
        public string Name { get; set; }
        public bool IsDefaultVariantLanguage { get; set; }
        public bool Mandatory { get; set; }
    }
}