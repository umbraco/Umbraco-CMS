using System.Runtime.Serialization;

namespace Umbraco.Web.Models.ContentEditing
{
    /// <summary>
    /// 
    /// </summary>
    [DataContract(Name = "content", Namespace = "")]
    public class LanguageForContentTranslation
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="LanguageForContentTranslation"/> class.
        /// </summary>
        public LanguageForContentTranslation()
        {
        }

        [DataMember(Name = "name")]
        public string Name { get; set; }

        [DataMember(Name = "isoCode")]
        public string IsoCode { get; set; }
    }
}
