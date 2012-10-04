using System;
using System.Collections.Generic;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public class DictionaryItem : Entity
    {
        private Guid _parentId;
        private string _itemKey;
        private IEnumerable<DictionaryTranslation> _translations;

        public DictionaryItem(Guid parentId, string itemKey)
        {
            _parentId = parentId;
            _itemKey = itemKey;
            _translations = new List<DictionaryTranslation>();
        }

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, Guid>(x => x.ParentId);
        private static readonly PropertyInfo ItemKeySelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, string>(x => x.ItemKey);
        private static readonly PropertyInfo TranslationsSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, IEnumerable<DictionaryTranslation>>(x => x.Translations);

        public Guid ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }
        
        public string ItemKey
        {
            get { return _itemKey; }
            set
            {
                _itemKey = value;
                OnPropertyChanged(ItemKeySelector);
            }
        }

        public IEnumerable<DictionaryTranslation> Translations
        {
            get { return _translations; }
            set
            {
                _translations = value;
                OnPropertyChanged(TranslationsSelector);
            }
        }
    }
}