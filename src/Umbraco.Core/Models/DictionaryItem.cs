using System;
using System.Collections.Generic;
using System.Linq;
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
    public class DictionaryItem : Entity, IDictionaryItem
    {
        public Func<int, ILanguage> GetLanguage { get; set; }
        private Guid? _parentId;
        private string _itemKey;
        private IEnumerable<IDictionaryTranslation> _translations;

        public DictionaryItem(string itemKey)
            : this(null, itemKey)
        {}

        public DictionaryItem(Guid? parentId, string itemKey)
        {
            _parentId = parentId;
            _itemKey = itemKey;
            _translations = new List<IDictionaryTranslation>();
        }

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, Guid?>(x => x.ParentId);
            public readonly PropertyInfo ItemKeySelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, string>(x => x.ItemKey);
            public readonly PropertyInfo TranslationsSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, IEnumerable<IDictionaryTranslation>>(x => x.Translations);
        }

        /// <summary>
        /// Gets or Sets the Parent Id of the Dictionary Item
        /// </summary>
        [DataMember]
        public Guid? ParentId
        {
            get { return _parentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentId, Ps.Value.ParentIdSelector); }
        }

        /// <summary>
        /// Gets or sets the Key for the Dictionary Item
        /// </summary>
        [DataMember]
        public string ItemKey
        {
            get { return _itemKey; }
            set { SetPropertyValueAndDetectChanges(value, ref _itemKey, Ps.Value.ItemKeySelector); }
        }

        /// <summary>
        /// Gets or sets a list of translations for the Dictionary Item
        /// </summary>
        [DataMember]
        public IEnumerable<IDictionaryTranslation> Translations
        {
            get { return _translations; }
            set
            {
                var asArray = value.ToArray();
                //ensure the language callback is set on each translation
                if (GetLanguage != null)
                {
                    foreach (var translation in asArray.OfType<DictionaryTranslation>())
                    {
                        translation.GetLanguage = GetLanguage;
                    }
                }

                SetPropertyValueAndDetectChanges(asArray, ref _translations, Ps.Value.TranslationsSelector,
                    //Custom comparer for enumerable
                    new DelegateEqualityComparer<IEnumerable<IDictionaryTranslation>>(
                        (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                        enumerable => enumerable.GetHashCode()));                
            }
        }
    }
}