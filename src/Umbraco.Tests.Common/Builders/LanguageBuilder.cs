using System;
using System.Globalization;
using Umbraco.Core.Configuration.Models;
using Umbraco.Core.Models;
using Umbraco.Tests.Common.Builders.Interfaces;

namespace Umbraco.Tests.Common.Builders
{
    public class LanguageBuilder : LanguageBuilder<object>
    {
        public LanguageBuilder() : base(null)
        {
        }
    }

    public class LanguageBuilder<TParent>
        : ChildBuilderBase<TParent, ILanguage>,
            IWithIdBuilder,
            IWithKeyBuilder,
            IWithCreateDateBuilder,
            IWithUpdateDateBuilder,
            IWithDeleteDateBuilder,
            IWithCultureInfoBuilder
    {
        private DateTime? _createDate;
        private CultureInfo _cultureInfo;
        private DateTime? _deleteDate;
        private int? _fallbackLanguageId;
        private int? _id;
        private bool? _isDefault;
        private bool? _isMandatory;
        private Guid? _key;
        private DateTime? _updateDate;

        public LanguageBuilder(TParent parentBuilder) : base(parentBuilder)
        {
        }

        public LanguageBuilder<TParent> WithIsDefault(bool isDefault)
        {
            _isDefault = isDefault;
            return this;
        }

        public LanguageBuilder<TParent> WithIsMandatory(bool isMandatory)
        {
            _isMandatory = isMandatory;
            return this;
        }

        public LanguageBuilder<TParent> WithFallbackLanguageId(int fallbackLanguageId)
        {
            _fallbackLanguageId = fallbackLanguageId;
            return this;
        }

        public override ILanguage Build()
        {
            var cultureInfo = _cultureInfo ?? CultureInfo.GetCultureInfo("en-US");
            var globalSettings = new GlobalSettings { DefaultUILanguage = cultureInfo.Name };
            var key = _key ?? Guid.NewGuid();
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var deleteDate = _deleteDate ?? null;
            var fallbackLanguageId = _fallbackLanguageId ?? null;
            var isDefault = _isDefault ?? false;
            var isMandatory = _isMandatory ?? false;

            return new Language(globalSettings, cultureInfo.Name)
            {
                Id = _id ?? 0,
                CultureName = cultureInfo.EnglishName,
                IsoCode = cultureInfo.Name,
                Key = key,
                CreateDate = createDate,
                UpdateDate = updateDate,
                DeleteDate = deleteDate,
                IsDefault = isDefault,
                IsMandatory = isMandatory,
                FallbackLanguageId = fallbackLanguageId
            };
        }

        DateTime? IWithCreateDateBuilder.CreateDate
        {
            get => _createDate;
            set => _createDate = value;
        }

        CultureInfo IWithCultureInfoBuilder.CultureInfo
        {
            get => _cultureInfo;
            set => _cultureInfo = value;
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
