using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Umbraco.Core.Models;

namespace Umbraco.Tests.Shared.Builders
{
    public class DictionaryItemBuilder
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

        public DictionaryItemBuilder WithCreateData(DateTime createDate)
        {
            _createDate = createDate;
            return this;
        }

        public DictionaryItemBuilder WithUpdateData(DateTime updateDate)
        {
            _updateDate = updateDate;
            return this;
        }

        public DictionaryItemBuilder WithId(int id)
        {
            _id = id;
            return this;
        }

        public DictionaryItemBuilder WithRandomTranslations(int count)
        {
            for (var i = 0; i < count; i++)
            {
                AddTranslation().Done();
            }
            return this;
        }
    }
}
