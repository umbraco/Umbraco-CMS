using System;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders.Interfaces;

namespace Umbraco.Tests.Shared.Builders
{
    public class DictionaryTranslationBuilder
        : ChildBuilderBase<DictionaryItemBuilder, IDictionaryTranslation>,
            IWithIdBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithDeleteDateBuilder,
            IWithKeyBuilder
    {
        private readonly LanguageBuilder<DictionaryTranslationBuilder> _languageBuilder;
        private readonly Guid? _uniqueId = null;
        private DateTime? _createDate;
        private DateTime? _deleteDate;
        private int? _id;
        private Guid? _key;
        private DateTime? _updateDate;
        private string _value;


        public DictionaryTranslationBuilder(DictionaryItemBuilder parentBuilder) : base(parentBuilder)
        {
            _languageBuilder = new LanguageBuilder<DictionaryTranslationBuilder>(this);
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        DateTime? IWithDeleteDateBuilder.DeleteDate
        {
            get => _deleteDate;
            set => _deleteDate = value;
        }

        int? IWithIdBuilder.Id
        {
            get => _id;
            set => _id = value;
        }

        Guid? IWithKeyBuilder.Key
        {
            get => _key;
            set => _key = value;
        }

        DateTime? IWithUpdateDateBuilder.UpdateDate
        {
            get => _updateDate;
            set => _updateDate = value;
        }

        public override IDictionaryTranslation Build()
        {
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var deleteDate = _deleteDate ?? null;
            var id = _id ?? 1;
            var key = _key ?? Guid.NewGuid();

            var result = new DictionaryTranslation(
                _languageBuilder.Build(),
                _value ?? Guid.NewGuid().ToString(),
                _uniqueId ?? key)
            {
                CreateDate = createDate,
                UpdateDate = updateDate,
                DeleteDate = deleteDate,
                Id = id
            };

            return result;
        }

        public LanguageBuilder<DictionaryTranslationBuilder> AddLanguage() => _languageBuilder;

        public DictionaryTranslationBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }
    }
}
