using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core;

namespace Umbraco.Tests.TestHelpers
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, /*AllowMultiple = false,*/ Inherited = false)]
    public class UmbracoTestAttribute : Attribute
    {
        private readonly Settable<bool> _autoMapper = new Settable<bool>();
        public bool AutoMapper { get { return _autoMapper.ValueOrDefault(false); } set { _autoMapper.Set(value);} }

        private readonly Settable<bool> _resetPluginManager = new Settable<bool>();
        public bool ResetPluginManager { get { return _resetPluginManager.ValueOrDefault(false); } set { _resetPluginManager.Set(value); } }

        private readonly Settable<bool> _facadeServiceRepositoryEvents = new Settable<bool>();
        public bool FacadeServiceRepositoryEvents { get { return _facadeServiceRepositoryEvents.ValueOrDefault(false); } set { _facadeServiceRepositoryEvents.Set(value); } }

        private readonly Settable<UmbracoTestOptions.Logger> _logger = new Settable<UmbracoTestOptions.Logger>();
        public UmbracoTestOptions.Logger Logger { get { return _logger.ValueOrDefault(UmbracoTestOptions.Logger.Mock); } set { _logger.Set(value); } }

        private readonly Settable<UmbracoTestOptions.Database> _database = new Settable<UmbracoTestOptions.Database>();
        public UmbracoTestOptions.Database Database { get { return _database.ValueOrDefault(UmbracoTestOptions.Database.None); } set { _database.Set(value); } }

        public static UmbracoTestAttribute Get(MethodInfo method)
        {
            var attr = ((UmbracoTestAttribute[]) method.GetCustomAttributes(typeof (UmbracoTestAttribute), true)).FirstOrDefault();
            var type = method.DeclaringType;
            return Get(type, attr);
        }

        public static UmbracoTestAttribute Get(Type type)
        {
            return Get(type, null);
        }

        private static UmbracoTestAttribute Get(Type type, UmbracoTestAttribute attr)
        {
            while (type != null && type != typeof(object))
            {
                var attr2 = ((UmbracoTestAttribute[]) type.GetCustomAttributes(typeof (UmbracoTestAttribute), true)).FirstOrDefault();
                if (attr2 != null)
                    attr = attr == null ? attr2 : attr2.Merge(attr);
                type = type.BaseType;
            }
            return attr ?? new UmbracoTestAttribute();
        }

        private UmbracoTestAttribute Merge(UmbracoTestAttribute other)
        {
            _autoMapper.Set(other._autoMapper);
            _resetPluginManager.Set(other._resetPluginManager);
            _facadeServiceRepositoryEvents.Set(other._facadeServiceRepositoryEvents);
            _logger.Set(other._logger);
            _database.Set(other._database);
            return this;
        }
    }
}
