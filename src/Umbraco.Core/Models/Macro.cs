using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.Serialization;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Macro
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    internal class Macro : Entity, IMacro
    {
        private string _alias;
        private string _name;
        private bool _useInEditor;
        private int _cacheDuration;
        private bool _cacheByPage;
        private bool _cacheByMember;
        private bool _dontRender;
        private string _scriptFile;
        private string _scriptAssembly;
        private string _python;
        private string _xslt;
        private List<IMacroProperty> _properties;

        private static readonly PropertyInfo AliasSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Alias);
        private static readonly PropertyInfo NameSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Name);
        private static readonly PropertyInfo UseInEditorSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.UseInEditor);
        private static readonly PropertyInfo CacheDurationSelector = ExpressionHelper.GetPropertyInfo<Macro, int>(x => x.CacheDuration);
        private static readonly PropertyInfo CacheByPageSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByPage);
        private static readonly PropertyInfo CacheByMemberSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.CacheByMember);
        private static readonly PropertyInfo DontRenderSelector = ExpressionHelper.GetPropertyInfo<Macro, bool>(x => x.DontRender);
        private static readonly PropertyInfo ScriptFileSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ScriptFile);
        private static readonly PropertyInfo ScriptAssemblySelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.ScriptAssembly);
        private static readonly PropertyInfo PythonSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Python);
        private static readonly PropertyInfo XsltSelector = ExpressionHelper.GetPropertyInfo<Macro, string>(x => x.Xslt);
        private static readonly PropertyInfo PropertiesSelector = ExpressionHelper.GetPropertyInfo<Macro, List<IMacroProperty>>(x => x.Properties);

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
                    _alias = value;
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
        /// Gets or sets the path to the script file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string ScriptFile
        {
            get { return _scriptFile; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _scriptFile = value;
                    return _scriptFile;
                }, _scriptFile, ScriptFileSelector);
            }
        }

        /// <summary>
        /// Gets or sets the name of the assembly, which should be used by the Macro
        /// </summary>
        /// <remarks>Will usually only be filled if the ScriptFile is a Usercontrol</remarks>
        [DataMember]
        public string ScriptAssembly
        {
            get { return _scriptAssembly; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _scriptAssembly = value;
                    return _scriptAssembly;
                }, _scriptAssembly, ScriptAssemblySelector);
            }
        }

        /// <summary>
        /// Gets or set the path to the Python file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string Python
        {
            get { return _python; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _python = value;
                    return _python;
                }, _python, PythonSelector);
            }
        }

        /// <summary>
        /// Gets or sets the path to the Xslt file in use
        /// </summary>
        /// <remarks>Optional: Can only be one of three Script, Python or Xslt</remarks>
        [DataMember]
        public string Xslt
        {
            get { return _xslt; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _xslt = value;
                    return _xslt;
                }, _xslt, XsltSelector);
            }
        }

        //TODO: This needs to be changed to a custom collection class so we can track the dirtyness of each property!
        /// <summary>
        /// Gets or sets a list of Macro Properties
        /// </summary>
        [DataMember]
        public List<IMacroProperty> Properties
        {
            get { return _properties; }
            set
            {
                SetPropertyValueAndDetectChanges(o =>
                {
                    _properties = value;
                    return _properties;
                }, _properties, PropertiesSelector);
            }
        }

        /// <summary>
        /// Overridden this method in order to set a random Id
        /// </summary>
        internal override void AddingEntity()
        {
            base.AddingEntity();

            var random = new Random();
            Id = random.Next(10000, int.MaxValue);

            Key = Alias.EncodeAsGuid();
        }

        /// <summary>
        /// Returns an enum <see cref="MacroTypes"/> based on the properties on the Macro
        /// </summary>
        /// <returns><see cref="MacroTypes"/></returns>
        public MacroTypes MacroType()
        {
            if (!string.IsNullOrEmpty(Xslt))
                return MacroTypes.Xslt;
            
            if (!string.IsNullOrEmpty(Python))
                return MacroTypes.Python;
            
            if (!string.IsNullOrEmpty(ScriptFile))
                return MacroTypes.Script;
            
            if (!string.IsNullOrEmpty(ScriptFile) && ScriptFile.ToLower().IndexOf(".ascx", StringComparison.InvariantCultureIgnoreCase) > -1)
            {
                return MacroTypes.UserControl;
            }
            
            if (!string.IsNullOrEmpty(ScriptFile) && !string.IsNullOrEmpty(ScriptAssembly))
                return MacroTypes.CustomControl;

            return MacroTypes.Unknown;
        }
    }
}