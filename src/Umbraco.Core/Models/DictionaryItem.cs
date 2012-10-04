using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Dictionary Item
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryItem : Entity
    {
        private Guid _parentId;
        private string _itemKey;
        private IEnumerable<DictionaryTranslation> _translations;
        //NOTE Add this to LocalizationService or Repository
        //private static Guid _topLevelParent = new Guid("41c7638d-f529-4bff-853e-59a0c2fb1bde");

        public DictionaryItem(Guid parentId, string itemKey)
        {
            _parentId = parentId;
            _itemKey = itemKey;
            _translations = new List<DictionaryTranslation>();
        }

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, Guid>(x => x.ParentId);
        private static readonly PropertyInfo ItemKeySelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, string>(x => x.ItemKey);
        private static readonly PropertyInfo TranslationsSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, IEnumerable<DictionaryTranslation>>(x => x.Translations);

        /// <summary>
        /// Gets or Sets the Parent Id of the Dictionary Item
        /// </summary>
        [DataMember]
        public Guid ParentId
        {
            get { return _parentId; }
            set
            {
                _parentId = value;
                OnPropertyChanged(ParentIdSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Key for the Dictionary Item
        /// </summary>
        [DataMember]
        public string ItemKey
        {
            get { return _itemKey; }
            set
            {
                _itemKey = value;
                OnPropertyChanged(ItemKeySelector);
            }
        }

        /// <summary>
        /// Gets or sets a list of translations for the Dictionary Item
        /// </summary>
        [DataMember]
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