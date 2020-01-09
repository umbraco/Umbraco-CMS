using System;
using System.Collections.Generic;
using System.Linq;
using Umbraco.Core.Models;
using Umbraco.Tests.Shared.Builders.Markers;

namespace Umbraco.Tests.Shared.Builders
{
    public class DictionaryItemBuilder : IWithIdBuilder, IWithCreateDateBuilder, IWithUpdateDateBuilder
    {
        private string _itemkey = null;
        private readonly List<DictionaryTranslationBuilder> _translationBuilders = new List<DictionaryTranslationBuilder>();
        private DateTime? _createDate;
        private DateTime? _updateDate;
        private int? _id = null;

        public DictionaryItem Build()
        {
            var createDate = _createDate ?? DateTime.Now;
            var updateDate = _updateDate ?? DateTime.Now;
            var id = _id ?? 1;

            var result = new DictionaryItem(_itemkey ?? Guid.NewGuid().ToString());
            result.Translations = _translationBuilders.Select(x => x.Build());
            result.CreateDate = createDate;
            result.UpdateDate = updateDate;
            result.Id = id;
            return result;
        }

        public DictionaryTranslationBuilder AddTranslation()
        {
            var builder = new DictionaryTranslationBuilder(this);

            _translationBuilders.Add(builder);

            return builder;
        }

        public DictionaryItemBuilder WithRandomTranslations(int count)
        {
            for (var i = 0; i < count; i++)
            {
                AddTranslation().Done();
            }
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
