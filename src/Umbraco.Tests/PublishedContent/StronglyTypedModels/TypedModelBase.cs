using System;
using System.Collections.Generic;
using System.Data.Entity.Design.PluralizationServices;
using System.Globalization;
using System.Reflection;
using System.Linq;
using Umbraco.Core;
using Umbraco.Core.Models;
using Umbraco.Web;

namespace Umbraco.Tests.PublishedContent.StronglyTypedModels
{
    /// <summary>
    /// Represents the abstract base class for a 'TypedModel', which basically wraps IPublishedContent
    /// underneath a strongly typed model like "Textpage" and "Subpage".
    /// Because IPublishedContent is used under the hood there is no need for additional mapping, so the
    /// only added cost should be the creation of the objects, which the IPublishedContent instance is 
    /// passed into.
    /// 
    /// This base class exposes a simple way to write property getters by convention without
    /// using the string alias of a PropertyType (this is resolved by the use of the Property delegate).
    /// 
    /// This base class also exposes query options like Parent, Children, Ancestors and Descendants,
    /// which can be used for collections of strongly typed child models/objects. These types of collections
    /// typically corresponds to 'allowed child content types' on a Doc Type (at different levels).
    /// 
    /// The IPublishedContent properties are also exposed through this base class, but only
    /// by casting the typed model to IPublishedContent, so the properties doesn't show up by default:
    /// ie. ((IPublishedContent)textpage).Url
    /// </summary>
    public abstract class TypedModelBase : IPublishedContent
    {
        private readonly IPublishedContent _publishedContent;

        protected TypedModelBase(IPublishedContent publishedContent)
        {
            _publishedContent = publishedContent;
        }

        protected readonly Func<MethodBase> Property = MethodBase.GetCurrentMethod;
        protected readonly Func<MethodBase> ContentTypeAlias = MethodBase.GetCurrentMethod;

        #region Properties

        protected T Resolve<T>(MethodBase methodBase)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return Resolve<T>(propertyTypeAlias);
        }

        protected T Resolve<T>(string propertyTypeAlias)
        {
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias);
        }

        protected T Resolve<T>(MethodBase methodBase, T ifCannotConvert)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return Resolve<T>(propertyTypeAlias, ifCannotConvert);
        }

        protected T Resolve<T>(string propertyTypeAlias, T ifCannotConvert)
        {
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias, false, ifCannotConvert);
        }

        protected T Resolve<T>(MethodBase methodBase, bool recursive, T ifCannotConvert)
        {
            var propertyTypeAlias = methodBase.ToUmbracoAlias();
            return Resolve<T>(propertyTypeAlias, recursive, ifCannotConvert);
        }

        protected T Resolve<T>(string propertyTypeAlias, bool recursive, T ifCannotConvert)
        {
            return _publishedContent.GetPropertyValue<T>(propertyTypeAlias, recursive, ifCannotConvert);
        }
        #endregion

        #region Querying
        protected T Parent<T>() where T : TypedModelBase
        {
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(IPublishedContent) });
            if (constructorInfo == null)
                throw new Exception("No valid constructor found");

            return (T) constructorInfo.Invoke(new object[] {_publishedContent.Parent});
        }

        protected IEnumerable<T> Children<T>(MethodBase methodBase) where T : TypedModelBase
        {
            var docTypeAlias = methodBase.CleanCallingMethodName();
            return Children<T>(docTypeAlias);
        }

        protected IEnumerable<T> Children<T>(string docTypeAlias) where T : TypedModelBase
        {
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(IPublishedContent) });
            if(constructorInfo == null)
                throw new Exception("No valid constructor found");

            string singularizedDocTypeAlias = docTypeAlias.ToSingular();

            return _publishedContent.Children.Where(x => x.DocumentTypeAlias == singularizedDocTypeAlias)
                .Select(x => (T)constructorInfo.Invoke(new object[] { x }));
        }

        protected IEnumerable<T> Ancestors<T>(MethodBase methodBase) where T : TypedModelBase
        {
            var docTypeAlias = methodBase.CleanCallingMethodName();
            return Ancestors<T>(docTypeAlias);
        }

        protected IEnumerable<T> Ancestors<T>(string docTypeAlias) where T : TypedModelBase
        {
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(IPublishedContent) });
            if (constructorInfo == null)
                throw new Exception("No valid constructor found");

            string singularizedDocTypeAlias = docTypeAlias.ToSingular();

            return _publishedContent.Ancestors().Where(x => x.DocumentTypeAlias == singularizedDocTypeAlias)
                .Select(x => (T)constructorInfo.Invoke(new object[] { x }));
        }

        protected IEnumerable<T> Descendants<T>(MethodBase methodBase) where T : TypedModelBase
        {
            var docTypeAlias = methodBase.CleanCallingMethodName();
            return Descendants<T>(docTypeAlias);
        }

        protected IEnumerable<T> Descendants<T>(string docTypeAlias) where T : TypedModelBase
        {
            var constructorInfo = typeof(T).GetConstructor(new[] { typeof(IPublishedContent) });
            if (constructorInfo == null)
                throw new Exception("No valid constructor found");

            string singularizedDocTypeAlias = docTypeAlias.ToSingular();

            return _publishedContent.Descendants().Where(x => x.DocumentTypeAlias == singularizedDocTypeAlias)
                .Select(x => (T)constructorInfo.Invoke(new object[] { x }));
        }
        #endregion

        #region IPublishedContent
        int IPublishedContent.Id { get { return _publishedContent.Id; } }
        int IPublishedContent.TemplateId { get { return _publishedContent.TemplateId; } }
        int IPublishedContent.SortOrder { get { return _publishedContent.SortOrder; } }
        string IPublishedContent.Name { get { return _publishedContent.Name; } }
        string IPublishedContent.UrlName { get { return _publishedContent.UrlName; } }
        string IPublishedContent.DocumentTypeAlias { get { return _publishedContent.DocumentTypeAlias; } }
        int IPublishedContent.DocumentTypeId { get { return _publishedContent.DocumentTypeId; } }
        string IPublishedContent.WriterName { get { return _publishedContent.WriterName; } }
        string IPublishedContent.CreatorName { get { return _publishedContent.CreatorName; } }
        int IPublishedContent.WriterId { get { return _publishedContent.WriterId; } }
        int IPublishedContent.CreatorId { get { return _publishedContent.CreatorId; } }
        string IPublishedContent.Path { get { return _publishedContent.Path; } }
        DateTime IPublishedContent.CreateDate { get { return _publishedContent.CreateDate; } }
        DateTime IPublishedContent.UpdateDate { get { return _publishedContent.UpdateDate; } }
        Guid IPublishedContent.Version { get { return _publishedContent.Version; } }
        int IPublishedContent.Level { get { return _publishedContent.Level; } }
        string IPublishedContent.Url { get { return _publishedContent.Url; } }
        PublishedItemType IPublishedContent.ItemType { get { return _publishedContent.ItemType; } }
        IPublishedContent IPublishedContent.Parent { get { return _publishedContent.Parent; } }
        IEnumerable<IPublishedContent> IPublishedContent.Children { get { return _publishedContent.Children; } }

        ICollection<IPublishedContentProperty> IPublishedContent.Properties { get { return _publishedContent.Properties; } }

        object IPublishedContent.this[string propertyAlias] { get { return _publishedContent[propertyAlias]; } }

        IPublishedContentProperty IPublishedContent.GetProperty(string alias)
        {
            return _publishedContent.GetProperty(alias);
        }
        #endregion
    }

    /// <summary>
    /// Extension methods for MethodBase, which are used to clean the name of the calling method "get_BodyText"
    /// to "BodyText" and then make it camel case according to the UmbracoAlias convention "bodyText".
    /// There is also a string extension for making plural words singular, which is used when going from
    /// something like "Subpages" to "Subpage" for Children/Ancestors/Descendants Doc Type aliases.
    /// </summary>
    public static class TypeExtensions
    {
        public static string CleanCallingMethodName(this MethodBase methodBase)
        {
            return methodBase.Name.Replace("get_", "");
        }

        public static string ToUmbracoAlias(this MethodBase methodBase)
        {
            return methodBase.CleanCallingMethodName().ToUmbracoAlias();
        }

        public static string ToSingular(this string pluralWord)
        {
            var service = PluralizationService.CreateService(new CultureInfo("en-US"));
            if (service.IsPlural(pluralWord))
                return service.Singularize(pluralWord);

            return pluralWord;
        }
    }
}