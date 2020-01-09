using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders.Interfaces;

namespace Umbraco.Tests.Shared.Builders
{
    public class DictionaryTranslationBuilder : ChildBuilderBase<DictionaryItemBuilder,IDictionaryTranslation>, IWithIdBuilder, IWithCreateDateBuilder, IWithUpdateDateBuilder
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

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }
    }
}
