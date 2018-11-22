using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Umbraco.Core.Dynamics;
using Umbraco.Core.Models;

namespace Umbraco.Tests.CodeFirst
{
    public abstract class ContentTypeBase
    {
        private IPublishedContent _content;

        protected ContentTypeBase(){}

        internal IPublishedContent Content
        {
            set { _content = value; }
        }

        #region Standard IPublisedContent properties

        public IPublishedContent Parent
        {
            get { return _content.Parent; }
        }

        public int Id
        {
            get { return _content.Id; }
        }
        public int TemplateId
        {
            get { return _content.TemplateId; }
        }
        public int SortOrder
        {
            get { return _content.SortOrder; }
        }
        public string Name
        {
            get { return _content.Name; }
        }
        public string UrlName
        {
            get { return _content.UrlName; }
        }
        public string DocumentTypeAlias
        {
            get { return _content.DocumentTypeAlias; }
        }
        public int DocumentTypeId
        {
            get { return _content.DocumentTypeId; }
        }
        public string WriterName
        {
            get { return _content.WriterName; }
        }
        public string CreatorName
        {
            get { return _content.CreatorName; }
        }
        public int WriterId
        {
            get { return _content.WriterId; }
        }
        public int CreatorId
        {
            get { return _content.CreatorId; }
        }
        public string Path
        {
            get { return _content.Path; }
        }
        public DateTime CreateDate
        {
            get { return _content.CreateDate; }
        }
        public DateTime UpdateDate
        {
            get { return _content.UpdateDate; }
        }
        public Guid Version
        {
            get { return _content.Version; }
        }
        public int Level
        {
            get { return _content.Level; }
        }

        //Using this attribute to hide Properties from Intellisense (when compiled?)
        [EditorBrowsable(EditorBrowsableState.Never)]
        public ICollection<IPublishedProperty> Properties
        {
            get { return _content.Properties; }
        }

        //Using this attribute to hide Properties from Intellisense (when compiled?)
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IEnumerable<IPublishedContent> Children
        {
            get { return _content.Children; }
        }

        //Using this attribute to hide Properties from Intellisense (when compiled?)
        [EditorBrowsable(EditorBrowsableState.Never)]
        public IPublishedProperty GetProperty(string alias)
        {
            return _content.GetProperty(alias);
        }

        #endregion

        #region Strongly typed queries
        public IEnumerable<T> ChildrenOfType<T>() where T : ContentTypeBase, new ()
        {
            var docTypeAlias = typeof (T).Name;
            return _content.Children
                .Where(x => x.DocumentTypeAlias == docTypeAlias)
                .Select(ContentTypeMapper.Map<T>);
        }
        public IEnumerable<T> Descendants<T>() where T : ContentTypeBase, new ()
        {
            var docTypeAlias = typeof(T).Name;
            return _content.Children.Map(x => true, (IPublishedContent n) => n.Children)
                .Where(x => x.DocumentTypeAlias == docTypeAlias)
                .Select(ContentTypeMapper.Map<T>);
        }
        #endregion
    }

    public static class ContentTypeBaseExtensions
    {
        public static IEnumerable<T> Children<T>(this ContentTypeBase contentTypeBase)
            where T : ContentTypeBase, new()
        {
            var docTypeAlias = typeof(T).Name;
            return contentTypeBase.Children
                .Where(x => x.DocumentTypeAlias == docTypeAlias)
                .Select(ContentTypeMapper.Map<T>);
        }

        public static IEnumerable<TSource> Map<TSource>(
            this IEnumerable<TSource> source,
            Func<TSource, bool> selectorFunction,
            Func<TSource, IEnumerable<TSource>> getChildrenFunction)
        {
            if (!source.Any())
            {
                return source;
            }
            // Add what we have to the stack   
            var flattenedList = source.Where(selectorFunction);
            // Go through the input enumerable looking for children,   
            // and add those if we have them   
            foreach (TSource element in source)
            {
                var secondInner = getChildrenFunction(element);
                if (secondInner.Any())
                {
                    secondInner = secondInner.Map(selectorFunction, getChildrenFunction);
                }
                flattenedList = flattenedList.Concat(secondInner);
            }
            return flattenedList;
        }
    }
}