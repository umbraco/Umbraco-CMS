using System;
using System.Reflection;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    public abstract class TypedModelBase
    {
        private IPublishedContent _publishedContent;

        protected TypedModelBase(){}

        protected TypedModelBase(IPublishedContent publishedContent)
        {
            _publishedContent = publishedContent;
        }

        internal void Add(IPublishedContent publishedContent)
        {
            _publishedContent = publishedContent;
        }

        public readonly Func<MethodBase> Property = MethodBase.GetCurrentMethod;
        public static string DefaultString = default(string);
        public static int DefaultInteger = default(int);
        public static bool DefaultBool = default(bool);
        public static DateTime DefaultDateTime = default(DateTime);

        public T Resolve<T>(MethodBase methodBase)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias);
        }

        public T Resolve<T>(MethodBase methodBase, T ifCannotConvert)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias, false, ifCannotConvert);
        }

        public T Resolve<T>(MethodBase methodBase, bool recursive, T ifCannotConvert)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias, recursive, ifCannotConvert);
        }

        public string ResolveString(MethodBase methodBase)
        {
            return Resolve<string>(methodBase);
        }

        public int ResolveInt(MethodBase methodBase)
        {
            return Resolve<int>(methodBase);
        }

        public bool ResolveBool(MethodBase methodBase)
        {
            return Resolve<bool>(methodBase);
        }

        public DateTime ResolveDate(MethodBase methodBase)
        {
            return Resolve<DateTime>(methodBase);
        }
    }

    public static class TypeExtensions
    {
        public static string ToUmbracoAlias(this MethodBase methodBase)
        {
            return methodBase.Name.Replace("get_", "").ToUmbracoAlias();
        }
    }
}