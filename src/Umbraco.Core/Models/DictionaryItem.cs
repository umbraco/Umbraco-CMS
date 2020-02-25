using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Dictionary Item
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class DictionaryItem : EntityBase, IDictionaryItem
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

        //Custom comparer for enumerable
        private static readonly DelegateEqualityComparer<IEnumerable<IDictionaryTranslation>> DictionaryTranslationComparer =
            new DelegateEqualityComparer<IEnumerable<IDictionaryTranslation>>(
                (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                enumerable => enumerable.GetHashCode());

        /// <summary>
        /// Gets or Sets the Parent Id of the Dictionary Item
        /// </summary>
        [DataMember]
        public Guid? ParentId
        {
            get { return _parentId; }
            set { SetPropertyValueAndDetectChanges(value, ref _parentId, nameof(ParentId)); }
        }

        /// <summary>
        /// Gets or sets the Key for the Dictionary Item
        /// </summary>
        [DataMember]
        public string ItemKey
        {
            get { return _itemKey; }
            set { SetPropertyValueAndDetectChanges(value, ref _itemKey, nameof(ItemKey)); }
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

                SetPropertyValueAndDetectChanges(asArray, ref _translations, nameof(Translations),
                    DictionaryTranslationComparer);
            }
        }
    }
}
