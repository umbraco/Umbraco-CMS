﻿using System;
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

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, Guid?>(x => x.ParentId);
        private static readonly PropertyInfo ItemKeySelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, string>(x => x.ItemKey);
        private static readonly PropertyInfo TranslationsSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, IEnumerable<IDictionaryTranslation>>(x => x.Translations);

        /// <summary>
        /// Gets or Sets the Parent Id of the Dictionary Item
        /// </summary>
        [DataMember]
        public Guid? ParentId
        {
            get { return _parentId; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _parentId = value;
                    return _parentId;
                }, _parentId, ParentIdSelector);
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
                SetPropertyValueAndDetectChanges(o =>
                {
                    _itemKey = value;
                    return _itemKey;
                }, _itemKey, ItemKeySelector);
            }
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
                SetPropertyValueAndDetectChanges(o =>
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

                    _translations = asArray;                    
                    return _translations;
                }, _translations, TranslationsSelector,
                    //Custom comparer for enumerable
                    new DelegateEqualityComparer<IEnumerable<IDictionaryTranslation>>(
                        (enumerable, translations) => enumerable.UnsortedSequenceEqual(translations),
                        enumerable => enumerable.GetHashCode()));
            }
        }
    }
}