using System.Globalization;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    public interface ILanguage : IEntity, IRememberBeingDirty
    {
        /// <summary>
        /// Gets or sets the Iso Code for the Language
        /// </summary>
        [DataMember]
        string IsoCode { get; set; }

        /// <summary>
        /// Gets or sets the Culture Name for the Language
        /// </summary>
        [DataMember]
        string CultureName { get; set; }

        /// <summary>
        /// Returns a <see cref="CultureInfo"/> object for the current Language
        /// </summary>
        [IgnoreDataMember]
        CultureInfo CultureInfo { get; }

        /// <summary>
        /// Defines if this language is the default variant language when language variants are in use
        /// </summary>
        bool IsDefaultVariantLanguage { get; set; }

        /// <summary>
        /// If true, a variant node cannot be published unless this language variant is created
        /// </summary>
        bool Mandatory { get; set; }

        /// <summary>
        /// Defines the id of a fallback language that can be used in multi-lingual scenarios to provide
        /// content if the requested language does not have it published.
        /// </summary>
        int? FallbackLanguageId { get; set; }
    }
}
