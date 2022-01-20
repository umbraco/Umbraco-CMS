// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Tests.Common.Builders.Interfaces;

namespace Umbraco.Cms.Tests.Common.Builders
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
        private DateTime? _createDate;
        private DateTime? _deleteDate;
        private int? _id;
        private Guid? _key;
        private DateTime? _updateDate;
        private string _value;

        public DictionaryTranslationBuilder()
            : base(null) => _languageBuilder = new LanguageBuilder<DictionaryTranslationBuilder>(this);

        public DictionaryTranslationBuilder(DictionaryItemBuilder parentBuilder)
            : base(parentBuilder) => _languageBuilder = new LanguageBuilder<DictionaryTranslationBuilder>(this);

        public LanguageBuilder<DictionaryTranslationBuilder> AddLanguage() => _languageBuilder;

        public DictionaryTranslationBuilder WithValue(string value)
        {
            _value = value;
            return this;
        }

        public override IDictionaryTranslation Build()
        {
            DateTime createDate = _createDate ?? DateTime.Now;
            DateTime updateDate = _updateDate ?? DateTime.Now;
            DateTime? deleteDate = _deleteDate ?? null;
            var id = _id ?? 1;
            Guid key = _key ?? Guid.NewGuid();

            var result = new DictionaryTranslation(
                _languageBuilder.Build(),
                _value ?? Guid.NewGuid().ToString(),
                key)
            {
                CreateDate = createDate,
                UpdateDate = updateDate,
                DeleteDate = deleteDate,
                Id = id
            };

            return result;
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
    }
}
