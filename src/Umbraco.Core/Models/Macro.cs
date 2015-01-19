using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using Umbraco.Core.IO;
using Umbraco.Core.Models.EntityBase;
using Umbraco.Core.Strings;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Macro : Entity, IMacro
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
        public Macro(int id, bool useInEditor, int cacheDuration, string @alias, string name, string controlType, string controlAssembly, string xsltPath, bool cacheByPage, bool cacheByMember, bool dontRender, string scriptPath)
            : this()
        {
            Id = id;
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

        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Alias);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Name);
        private static readonly PropertyInfo UseInEditorSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.UseInEditor);
        private static readonly PropertyInfo CacheDurationSelector = ExpressionHelper.GetPropertyInfo<Macro, int>(x => x.CacheDuration);
        private static readonly PropertyInfo CacheByPageSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByPage);
        private static readonly PropertyInfo CacheByMemberSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByMember);
        private static readonly PropertyInfo DontRenderSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.DontRender);
        private static readonly PropertyInfo ControlPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ControlType);
        private static readonly PropertyInfo ControlAssemblySelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ControlAssembly);
        private static readonly PropertyInfo ScriptPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ScriptPath);
        private static readonly PropertyInfo XsltPathSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.XsltPath);
        private static readonly PropertyInfo PropertiesSelector = ExpressionHelper.GetPropertyInfo<Macro, MacroPropertyCollection>(x => x.Properties);

        void PropertiesChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            OnPropertyChanged(PropertiesSelector);

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
            OnPropertyChanged(PropertiesSelector);
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
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _alias = value.ToCleanString(CleanStringType.Alias);
                    return _alias;
                }, _alias, AliasSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the Macro
        /// </summary>
        [DataMember]
        public string Name
        {
            get { return _name; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _name = value;
                    return _name;
                }, _name, NameSelector);
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro can be used in an Editor
        /// </summary>
        [DataMember]
        public bool UseInEditor
        {
            get { return _useInEditor; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _useInEditor = value;
                    return _useInEditor;
                }, _useInEditor, UseInEditorSelector);
            }
        }

        /// <summary>
        /// Gets or sets the Cache Duration for the Macro
        /// </summary>
        [DataMember]
        public int CacheDuration
        {
            get { return _cacheDuration; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _cacheDuration = value;
                    return _cacheDuration;
                }, _cacheDuration, CacheDurationSelector);
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached by Page
        /// </summary>
        [DataMember]
        public bool CacheByPage
        {
            get { return _cacheByPage; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _cacheByPage = value;
                    return _cacheByPage;
                }, _cacheByPage, CacheByPageSelector);
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be Cached Personally
        /// </summary>
        [DataMember]
        public bool CacheByMember
        {
            get { return _cacheByMember; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _cacheByMember = value;
                    return _cacheByMember;
                }, _cacheByMember, CacheByMemberSelector);
            }
        }

        /// <summary>
        /// Gets or sets a boolean indicating whether the Macro should be rendered in an Editor
        /// </summary>
        [DataMember]
        public bool DontRender
        {
            get { return _dontRender; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _dontRender = value;
                    return _dontRender;
                }, _dontRender, DontRenderSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path to user control or the Control Type to render
        /// </summary>
        [DataMember]
        public string ControlType
        {
            get { return _scriptFile; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _scriptFile = value;
                    return _scriptFile;
                }, _scriptFile, ControlPathSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the assembly, which should be used by the Macro
        /// </summary>
        /// <remarks>Will usually only be filled if the ControlType is a Usercontrol</remarks>
        [DataMember]
        public string ControlAssembly
        {
            get { return _scriptAssembly; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _scriptAssembly = value;
                    return _scriptAssembly;
                }, _scriptAssembly, ControlAssemblySelector);
            }
        }

        /// <summary>
        /// Gets or set the path to the Python file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string ScriptPath
        {
            get { return _scriptPath; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _scriptPath = value;
                    return _scriptPath;
                }, _scriptPath, ScriptPathSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path to the Xslt file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string XsltPath
        {
            get { return _xslt; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _xslt = value;
                    return _xslt;
                }, _xslt, XsltPathSelector);
            }
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

            clone._addedProperties = new List<string>();
            clone._removedProperties = new List<string>();
            clone._properties = (MacroPropertyCollection)Properties.DeepClone();
            //re-assign the event handler
            clone._properties.CollectionChanged += clone.PropertiesChanged;

            clone.ResetDirtyProperties(false);

            return clone;
        }
    }
}