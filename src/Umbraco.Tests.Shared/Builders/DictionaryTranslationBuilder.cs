using System;
using System.Globalization;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Shared.Builders
{
    public class DictionaryTranslationBuilder : ChildBuilderBase<DictionaryItemBuilder,IDictionaryTranslation>
    {
        private string _value = null;
        private Guid? _uniqueId = null;
        private DateTime? _createDate;
        private DateTime? _updateDate;


        private LanguageBuilder<DictionaryTranslationBuilder> _languageBuilder;
        private int? _id = null;

        public DictionaryTranslationBuilder(DictionaryItemBuilder parentBuilder) : base(parentBuilder)
        {
            _languageBuilder = new LanguageBuilder<DictionaryTranslationBuilder>(this);

        }

        public override IDictionaryTranslation Build()
        {
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var id = _id ?? 1;

            var result =  new DictionaryTranslation(
                _languageBuilder.Build(),
                _value ?? Guid.NewGuid().ToString(),
                _uniqueId ?? Guid.NewGuid());

            result.CreateDate = createDate;
            result.UpdateDate = updateDate;
            result.Id = id;

            return result;
        }

        public LanguageBuilder<DictionaryTranslationBuilder> WithLanguage()
        {
            return _languageBuilder;
        }

        public DictionaryTranslationBuilder  WithValue(string value)
        {
            _value = value;
            return this;
        }

        public DictionaryTranslationBuilder WithCreateData(DateTime createDate)
        {
            _createDate = createDate;
            return this;
        }

        public DictionaryTranslationBuilder WithUpdateData(DateTime updateDate)
        {
            _updateDate = updateDate;
            return this;
        }

        public DictionaryTranslationBuilder WithId(int id)
        {
            _id = id;
            return this;
        }
    }
}
