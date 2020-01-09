using System.Globalization;
using Moq;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Shared.Builders
{
    public class LanguageBuilder : LanguageBuilder<object>
    {
        public LanguageBuilder() : base(null)
        {
        }
    }


    public class LanguageBuilder<TParent> : ChildBuilderBase<TParent, ILanguage>, IWithIdBuilder
    {

        private string _isoCode = null;
        private int? _id;

        public LanguageBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public override ILanguage Build()
        {
            var culture = CultureInfo.GetCultureInfo("en-US");
            var isoCode = _isoCode ?? culture.Name;
            return new Language(Mock.Of<IGlobalSettings>(), isoCode)
            {
                Id = _id ?? 1,
                CultureName = culture.TwoLetterISOLanguageName,
                IsoCode =  new RegionInfo(culture.LCID).Name,
            };
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }
    }
}
