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
        private Guid _parentId;
        private string _itemKey;
        private IEnumerable<IDictionaryTranslation> _translations;

        public DictionaryItem(string itemKey)
            : this(new Guid(Constants.Conventions.Localization.DictionaryItemRootId), itemKey)
        {}

        public DictionaryItem(Guid parentId, string itemKey)
        {
            _parentId = parentId;
            _itemKey = itemKey;
            _translations = new List<IDictionaryTranslation>();
        }

        private static readonly PropertyInfo ParentIdSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, Guid>(x => x.ParentId);
        private static readonly PropertyInfo ItemKeySelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, string>(x => x.ItemKey);
        private static readonly PropertyInfo TranslationsSelector = ExpressionHelper.GetPropertyInfo<DictionaryItem, IEnumerable<IDictionaryTranslation>>(x => x.Translations);

        /// <summary>
        /// Gets or Sets the Parent Id of the Dictionary Item
        /// </summary>
        [DataMember]
        public Guid ParentId
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
                    _translations = value;
                    return _translations;
                }, _translations, TranslationsSelector);
            }
        }

        /// <summary>
        /// Method to call before inserting a new entity in the db
        /// </summary>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            Key = Guid.NewGuid();

            //If ParentId is not set we should default to the root parent id
            if(ParentId == Guid.Empty)
                _parentId = new Guid(Constants.Conventions.Localization.DictionaryItemRootId);
        }
        
    }
}