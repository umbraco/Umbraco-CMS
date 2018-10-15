﻿using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Macro : Entity, IMacro
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
        /// <param name="controlType"></param>
        /// <param name="controlAssembly"></param>
        /// <param name="xsltPath"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cacheByMember"></param>
        /// <param name="dontRender"></param>
        /// <param name="scriptPath"></param>
        public Macro(int id, Guid key, bool useInEditor, int cacheDuration, string @alias, string name, string controlType, string controlAssembly, string xsltPath, bool cacheByPage, bool cacheByMember, bool dontRender, string scriptPath)
            : this()
        {
            Id = id;
            Key = key;
            UseInEditor = useInEditor;
            CacheDuration = cacheDuration;
            Alias = alias.ToCleanString(CleanStringType.Alias);
            Name = name;
            ControlType = controlType;
            ControlAssembly = controlAssembly;
            XsltPath = xsltPath;
            CacheByPage = cacheByPage;
            CacheByMember = cacheByMember;
            DontRender = dontRender;
            ScriptPath = scriptPath;
        }

        /// <summary>
        /// Creates an instance for persisting a new item
        /// </summary>
        /// <param name="useInEditor"></param>
        /// <param name="cacheDuration"></param>
        /// <param name="alias"></param>
        /// <param name="name"></param>
        /// <param name="controlType"></param>
        /// <param name="controlAssembly"></param>
        /// <param name="xsltPath"></param>
        /// <param name="cacheByPage"></param>
        /// <param name="cacheByMember"></param>
        /// <param name="dontRender"></param>
        /// <param name="scriptPath"></param>
        public Macro(string @alias, string name,
            string controlType = "",
            string controlAssembly = "",
            string xsltPath = "",
            string scriptPath = "",
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
            ControlType = controlType;
            ControlAssembly = controlAssembly;
            XsltPath = xsltPath;
            CacheByPage = cacheByPage;
            CacheByMember = cacheByMember;
            DontRender = dontRender;
            ScriptPath = scriptPath;
        }

        private string _alias;
        private string _name;
        private bool _useInEditor;
        private int _cacheDuration;
        private bool _cacheByPage;
        private bool _cacheByMember;
        private bool _dontRender;
        private string _scriptFile;
        private string _scriptAssembly;
        private string _scriptPath;
        private string _xslt;
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
            public readonly PropertyInfo ControlPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ControlType);
            public readonly PropertyInfo ControlAssemblySelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ControlAssembly);
            public readonly PropertyInfo ScriptPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ScriptPath);
            public readonly PropertyInfo XsltPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.XsltPath);
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
        
        public override void ResetDirtyProperties(bool rememberPreviouslyChangedProperties)
        {
            _addedProperties.Clear();
            _removedProperties.Clear();
            base.ResetDirtyProperties(rememberPreviouslyChangedProperties);
            foreach (var prop in Properties)
            {
                ((TracksChangesEntityBase)prop).ResetDirtyProperties(rememberPreviouslyChangedProperties);
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
        /// Gets or sets the path to user control or the Control Type to render
        /// </summary>
        [DataMember]
        public string ControlType
        {
            get { return _scriptFile; }
            set { SetPropertyValueAndDetectChanges(value, ref _scriptFile, Ps.Value.ControlPathSelector); }
        }

        /// <summary>
        /// Gets or sets the name of the assembly, which should be used by the Macro
        /// </summary>
        /// <remarks>Will usually only be filled if the ControlType is a Usercontrol</remarks>
        [DataMember]
        public string ControlAssembly
        {
            get { return _scriptAssembly; }
            set { SetPropertyValueAndDetectChanges(value, ref _scriptAssembly, Ps.Value.ControlAssemblySelector); }
        }

        /// <summary>
        /// Gets or set the path to the Python file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string ScriptPath
        {
            get { return _scriptPath; }
            set { SetPropertyValueAndDetectChanges(value, ref _scriptPath, Ps.Value.ScriptPathSelector); }
        }

        /// <summary>
        /// Gets or sets the path to the Xslt file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string XsltPath
        {
            get { return _xslt; }
            set { SetPropertyValueAndDetectChanges(value, ref _xslt, Ps.Value.XsltPathSelector); }
        }

        /// <summary>
        /// Gets or sets a list of Macro Properties
        /// </summary>
        [DataMember]
        public MacroPropertyCollection Properties
        {
            get { return _properties; }            
        }

        public override object DeepClone()
        {
            var clone = (Macro)base.DeepClone();
            //turn off change tracking
            clone.DisableChangeTracking();
            clone._addedProperties = new List<string>();
            clone._removedProperties = new List<string>();
            clone._properties = (MacroPropertyCollection)Properties.DeepClone();
            //re-assign the event handler
            clone._properties.CollectionChanged += clone.PropertiesChanged;
            //this shouldn't really be needed since we're not tracking
            clone.ResetDirtyProperties(false);
            //re-enable tracking
            clone.EnableChangeTracking();

            return clone;
        }
    }
}