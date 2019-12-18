using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.ModelsBuilder.Building
{
    /// <summary>
    /// Contains the result of a code parsing.
    /// </summary>
    internal class ParseResult
    {
        // "alias" is the umbraco alias
        // content "name" is the complete name eg Foo.Bar.Name
        // property "name" is just the local name

        // see notes in IgnoreContentTypeAttribute

        private readonly HashSet<string> _ignoredContent 
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        //private readonly HashSet<string> _ignoredMixin
        //    = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        //private readonly HashSet<string> _ignoredMixinProperties
        //    = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, string> _renamedContent 
            = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly HashSet<string> _withImplementContent
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
        private readonly Dictionary<string, HashSet<string>> _ignoredProperty
            = new Dictionary<string, HashSet<string>>();
        private readonly Dictionary<string, Dictionary<string, string>> _renamedProperty
            = new Dictionary<string, Dictionary<string, string>>();
        private readonly Dictionary<string, string> _contentBase
            = new Dictionary<string, string>();
        private readonly Dictionary<string, string[]> _contentInterfaces 
            = new Dictionary<string, string[]>();
        private readonly List<string> _usingNamespaces
            = new List<string>();
        private readonly Dictionary<string, List<StaticMixinMethodInfo>> _staticMixins
            = new Dictionary<string, List<StaticMixinMethodInfo>>();
        private readonly HashSet<string> _withCtor
            = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);

        public static readonly ParseResult Empty = new ParseResult();

        private class StaticMixinMethodInfo
        {
            public StaticMixinMethodInfo(string contentName, string methodName, string returnType, string paramType)
            {
                ContentName = contentName;
                MethodName = methodName;
                //ReturnType = returnType;
                //ParamType = paramType;
            }

            // short name eg Type1
            public string ContentName { get; private set; }

            // short name eg GetProp1
            public string MethodName { get; private set; }

            // those types cannot be FQ because when parsing, some of them
            // might not exist since we're generating them... and so prob.
            // that info is worthless - not using it anyway at the moment...

            //public string ReturnType { get; private set; }
            //public string ParamType { get; private set; }
        }

        #region Declare

        // content with that alias should not be generated
        // alias can end with a * (wildcard)
        public void SetIgnoredContent(string contentAlias /*, bool ignoreContent, bool ignoreMixin, bool ignoreMixinProperties*/)
        {
            //if (ignoreContent)
                _ignoredContent.Add(contentAlias);
            //if (ignoreMixin)
            //    _ignoredMixin.Add(contentAlias);
            //if (ignoreMixinProperties)
            //    _ignoredMixinProperties.Add(contentAlias);
        }

        // content with that alias should be generated with a different name
        public void SetRenamedContent(string contentAlias, string contentName, bool withImplement)
        {
            _renamedContent[contentAlias] = contentName;
            if (withImplement)
                _withImplementContent.Add(contentAlias);
        }

        // property with that alias should not be generated
        // applies to content name and any content that implements it
        // here, contentName may be an interface
        // alias can end with a * (wildcard)
        public void SetIgnoredProperty(string contentName, string propertyAlias)
        {
            HashSet<string> ignores;
            if (!_ignoredProperty.TryGetValue(contentName, out ignores))
                ignores = _ignoredProperty[contentName] = new HashSet<string>(StringComparer.InvariantCultureIgnoreCase);
            ignores.Add(propertyAlias);
        }

        // property with that alias should be generated with a different name
        // applies to content name and any content that implements it
        // here, contentName may be an interface
        public void SetRenamedProperty(string contentName, string propertyAlias, string propertyName)
        {
            Dictionary<string, string> renames;
            if (!_renamedProperty.TryGetValue(contentName, out renames))
                renames = _renamedProperty[contentName] = new Dictionary<string, string>(StringComparer.InvariantCultureIgnoreCase);
            renames[propertyAlias] = propertyName;
        }

        // content with that name has a base class so no need to generate one
        public void SetContentBaseClass(string contentName, string baseName)
        {
            if (baseName.ToLowerInvariant() != "object")
                _contentBase[contentName] = baseName;
        }

        // content with that name implements the interfaces
        public void SetContentInterfaces(string contentName, IEnumerable<string> interfaceNames)
        {
            _contentInterfaces[contentName] = interfaceNames.ToArray();
        }

        public void SetModelsBaseClassName(string modelsBaseClassName)
        {
            ModelsBaseClassName = modelsBaseClassName;
        }

        public void SetModelsNamespace(string modelsNamespace)
        {
            ModelsNamespace = modelsNamespace;
        }

        public void SetUsingNamespace(string usingNamespace)
        {
            _usingNamespaces.Add(usingNamespace);
        }

        public void SetStaticMixinMethod(string contentName, string methodName, string returnType, string paramType)
        {
            if (!_staticMixins.ContainsKey(contentName))
                _staticMixins[contentName] = new List<StaticMixinMethodInfo>();

            _staticMixins[contentName].Add(new StaticMixinMethodInfo(contentName, methodName, returnType, paramType));
        }

        public void SetHasCtor(string contentName)
        {
            _withCtor.Add(contentName);
        }

        #endregion

        #region Query

        public bool IsIgnored(string contentAlias)
        {
            return IsContentOrMixinIgnored(contentAlias, _ignoredContent);
        }

        //public bool IsMixinIgnored(string contentAlias)
        //{
        //    return IsContentOrMixinIgnored(contentAlias, _ignoredMixin);
        //}
        
        //public bool IsMixinPropertiesIgnored(string contentAlias)
        //{
        //    return IsContentOrMixinIgnored(contentAlias, _ignoredMixinProperties);
        //}

        private static bool IsContentOrMixinIgnored(string contentAlias, HashSet<string> ignored)
        {
            if (ignored.Contains(contentAlias)) return true;
            return ignored
                .Where(x => x.EndsWith("*"))
                .Select(x => x.Substring(0, x.Length - 1))
                .Any(x => contentAlias.StartsWith(x, StringComparison.InvariantCultureIgnoreCase));
        }

        public bool HasContentBase(string contentName)
        {
            return _contentBase.ContainsKey(contentName);
        }

        public bool IsContentRenamed(string contentAlias)
        {
            return _renamedContent.ContainsKey(contentAlias);
        }

        public bool HasContentImplement(string contentAlias)
        {
            return _withImplementContent.Contains(contentAlias);
        }

        public string ContentClrName(string contentAlias)
        {
            string name;
            return (_renamedContent.TryGetValue(contentAlias, out name)) ? name : null;
        }

        public bool IsPropertyIgnored(string contentName, string propertyAlias)
        {
            HashSet<string> ignores;
            if (_ignoredProperty.TryGetValue(contentName, out ignores))
            {
                if (ignores.Contains(propertyAlias)) return true;
                if (ignores
                        .Where(x => x.EndsWith("*"))
                        .Select(x => x.Substring(0, x.Length - 1))
                        .Any(x => propertyAlias.StartsWith(x, StringComparison.InvariantCultureIgnoreCase)))
                    return true;
            }
            string baseName;
            if (_contentBase.TryGetValue(contentName, out baseName)
                && IsPropertyIgnored(baseName, propertyAlias)) return true;
            string[] interfaceNames;
            if (_contentInterfaces.TryGetValue(contentName, out interfaceNames)
                && interfaceNames.Any(interfaceName => IsPropertyIgnored(interfaceName, propertyAlias))) return true;
            return false;
        }

        public string PropertyClrName(string contentName, string propertyAlias)
        {
            Dictionary<string, string> renames;
            string name;
            if (_renamedProperty.TryGetValue(contentName, out renames)
                && renames.TryGetValue(propertyAlias, out name)) return name;
            string baseName;
            if (_contentBase.TryGetValue(contentName, out baseName)
                && null != (name = PropertyClrName(baseName, propertyAlias))) return name;
            string[] interfaceNames;
            if (_contentInterfaces.TryGetValue(contentName, out interfaceNames)
                && null != (name = interfaceNames
                    .Select(interfaceName => PropertyClrName(interfaceName, propertyAlias))
                    .FirstOrDefault(x => x != null))) return name;
            return null;
        }

        public bool HasModelsBaseClassName
        {
            get { return !string.IsNullOrWhiteSpace(ModelsBaseClassName); }
        }

        public string ModelsBaseClassName { get; private set; }

        public bool HasModelsNamespace
        {
            get { return !string.IsNullOrWhiteSpace(ModelsNamespace); }
        }

        public string ModelsNamespace { get; private set; }

        public IEnumerable<string> UsingNamespaces
        {
            get { return _usingNamespaces; }
        }

        public IEnumerable<string> StaticMixinMethods(string contentName)
        {
            return _staticMixins.ContainsKey(contentName)
                ? _staticMixins[contentName].Select(x => x.MethodName)
                : Enumerable.Empty<string>() ;
        }

        public bool HasCtor(string contentName)
        {
            return _withCtor.Contains(contentName);
        }

        #endregion
    }
}