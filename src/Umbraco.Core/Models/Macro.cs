using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.Entities;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Macro : EntityBase, IMacro
    {
        public Macro()
        {
            _properties = new MacroPropertyCollection();
            _properties.CollectionChanged += PropertiesChanged;
            _addedProperties = new List<string>();
            _removedProperties = new List<string>();
        }

        /// <summary>
        /// Creates an item with pre-filled properties
        /// </summary>
        /// <param name="id"></param>
        /// <param name="key"></param>
        /// <param name="useInEditor"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cacheByMember"></param>
        /// <param name="dontRender"></param>
        /// <param name="macroSource"></param>
        public Macro(int id, Guid key, bool useInEditor, int cacheDuration, string @alias, string name, bool cacheByPage, bool cacheByMember, bool dontRender, string macroSource, MacroTypes macroType)
            : this()
        {
            Id = id;
            Key = key;
            UseInEditor = useInEditor;
            CacheDuration = cacheDuration;
            Alias = alias.ToCleanString(CleanStringType.Alias);
            Name = name;
            CacheByPage = cacheByPage;
            CacheByMember = cacheByMember;
            DontRender = dontRender;
            MacroSource = macroSource;
            MacroType = macroType;
        }

        /// <summary>
        /// Creates an instance for persisting a new item
        /// </summary>
        /// <param name="useInEditor"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cacheByMember"></param>
        /// <param name="dontRender"></param>
        /// <param name="macroSource"></param>
        public Macro(string @alias, string name,
            string macroSource,
            MacroTypes macroType,
            bool cacheByPage = false,
            bool cacheByMember = false,
            bool dontRender = true,
            bool useInEditor = false,
            int cacheDuration = 0)
            : this()
        {
            UseInEditor = useInEditor;
            CacheDuration = cacheDuration;
            Alias = alias.ToCleanString(CleanStringType.Alias);
            Name = name;
            CacheByPage = cacheByPage;
            CacheByMember = cacheByMember;
            DontRender = dontRender;
            MacroSource = macroSource;
            MacroType = macroType;
        }

        private string _alias;
        private string _name;
        private bool _useInEditor;
        private int _cacheDuration;
        private bool _cacheByPage;
        private bool _cacheByMember;
        private bool _dontRender;
        private string _macroSource;
        private MacroTypes _macroType = MacroTypes.Unknown;
        private MacroPropertyCollection _properties;
        private List<string> _addedProperties;
        private List<string> _removedProperties;

        private static readonly Lazy<PropertySelectors> Ps = new Lazy<PropertySelectors>();

        private class PropertySelectors
        {
            public readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Alias);
            public readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Name);
            public readonly PropertyInfo UseInEditorSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.UseInEditor);
            public readonly PropertyInfo CacheDurationSelector = ExpressionHelper.GetPropertyInfo<Macro, int>(x => x.CacheDuration);
            public readonly PropertyInfo CacheByPageSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByPage);
            public readonly PropertyInfo CacheByMemberSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByMember);
            public readonly PropertyInfo DontRenderSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.DontRender);
            public readonly PropertyInfo ScriptPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.MacroSource);
            public readonly PropertyInfo MacroTypeSelector = ExpressionHelper.GetPropertyInfo<Macro, MacroTypes>(x => x.MacroType);
            public readonly PropertyInfo PropertiesSelector = ExpressionHelper.GetPropertyInfo<Macro, MacroPropertyCollection>(x => x.Properties);
        }

        void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertiesSelector);

            if (e.Action == NotifyCollectionChangedAction.Add)
            {
                //listen for changes
                var prop = e.NewItems.Cast<MacroProperty>().First();
                prop.PropertyChanged += PropertyDataChanged;

                var alias = prop.Alias;

                if (_addedProperties.Contains(alias) == false)
                {
                    //add to the added props
                    _addedProperties.Add(alias);
                }
            }
            else if (e.Action == NotifyCollectionChangedAction.Remove)
            {
                //remove listening for changes
                var prop = e.OldItems.Cast<MacroProperty>().First();
                prop.PropertyChanged -= PropertyDataChanged;

                var alias = prop.Alias;

                if (_removedProperties.Contains(alias) == false)
                {
                    _removedProperties.Add(alias);
                }
            }
        }

        /// <summary>
        /// When some data of a property has changed ensure our Properties flag is dirty
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void PropertyDataChanged(object sender, PropertyChangedEventArgs e)
        {
            OnPropertyChanged(Ps.Value.PropertiesSelector);
        }

        public override void ResetDirtyProperties(bool rememberDirty)
        {
            _addedProperties.Clear();
            _removedProperties.Clear();
            base.ResetDirtyProperties(rememberDirty);
            foreach (var prop in Properties)
            {
                ((BeingDirtyBase)prop).ResetDirtyProperties(rememberDirty);
            }
        }

        /// <summary>
        /// Used internally to check if we need to add a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> AddedProperties
        {
            get { return _addedProperties; }
        }

        /// <summary>
        /// Used internally to check if we need to remove  a section in the repository to the db
        /// </summary>
        internal IEnumerable<string> RemovedProperties
        {
            get { return _removedProperties; }
        }

        /// <summary>
        /// Gets or sets the alias of the Macro
        /// </summary>
        [DataMember]
        public string Alias
        {
            get { return _alias; }
            set { SetPropertyValueAndDetectChanges(value.ToCleanString(CleanStringType.Alias), ref _alias, Ps.Value.AliasSelector); }
        }

        /// <summary>
        /// Gets or sets the name of the Macro
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set { SetPropertyValueAndDetectChanges(value, ref _name, Ps.Value.NameSelector); }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro can be used in an Editor
        /// </summary>
        [DataMember]
        public bool UseInEditor
        {
            get { return _useInEditor; }
            set { SetPropertyValueAndDetectChanges(value, ref _useInEditor, Ps.Value.UseInEditorSelector); }
        }

        /// <summary>
        /// Gets or sets the Cache Duration for the Macro
        /// </summary>
        [DataMember]
        public int CacheDuration
        {
            get { return _cacheDuration; }
            set { SetPropertyValueAndDetectChanges(value, ref _cacheDuration, Ps.Value.CacheDurationSelector); }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached by Page
        /// </summary>
        [DataMember]
        public bool CacheByPage
        {
            get { return _cacheByPage; }
            set { SetPropertyValueAndDetectChanges(value, ref _cacheByPage, Ps.Value.CacheByPageSelector); }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached Personally
        /// </summary>
        [DataMember]
        public bool CacheByMember
        {
            get { return _cacheByMember; }
            set { SetPropertyValueAndDetectChanges(value, ref _cacheByMember, Ps.Value.CacheByMemberSelector); }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be rendered in an Editor
        /// </summary>
        [DataMember]
        public bool DontRender
        {
            get { return _dontRender; }
            set { SetPropertyValueAndDetectChanges(value, ref _dontRender, Ps.Value.DontRenderSelector); }
        }

        /// <summary>
        /// Gets or set the path to the Partial View to render
        /// </summary>
        [DataMember]
        public string MacroSource
        {
            get { return _macroSource; }
            set { SetPropertyValueAndDetectChanges(value, ref _macroSource, Ps.Value.ScriptPathSelector); }
        }

        /// <summary>
        /// Gets or set the path to the Partial View to render
        /// </summary>
        [DataMember]
        public MacroTypes MacroType
        {
            get { return _macroType; }
            set { SetPropertyValueAndDetectChanges(value, ref _macroType, Ps.Value.MacroTypeSelector); }
        }

        /// <summary>
        /// Gets or sets a list of Macro Properties
        /// </summary>
        [DataMember]
        public MacroPropertyCollection Properties
        {
            get { return _properties; }
        }

        protected override void PerformDeepClone(object clone)
        {
            base.PerformDeepClone(clone);

            var clonedEntity = (Macro)clone;
            
            clonedEntity._addedProperties = new List<string>();
            clonedEntity._removedProperties = new List<string>();
            clonedEntity._properties = (MacroPropertyCollection)Properties.DeepClone();
            //re-assign the event handler
            clonedEntity._properties.CollectionChanged += clonedEntity.PropertiesChanged;
            
        }
    }
}
