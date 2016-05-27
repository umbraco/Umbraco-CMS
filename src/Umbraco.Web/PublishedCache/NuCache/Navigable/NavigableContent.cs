using System;
using System.Collections.Generic;
using Umbraco.Core.Models;
using Umbraco.Core.Xml.XPath;

namespace Umbraco.Web.PublishedCache.NuCache.Navigable
{
    class NavigableContent : INavigableContent
    {
        private readonly IPublishedContent _icontent;
        private readonly PublishedContent _content;
        //private readonly object[] _builtInValues1;
        private readonly string[] _builtInValues;

        public NavigableContent(IPublishedContent content)
        {
            _icontent = content;
            _content = PublishedContent.UnwrapIPublishedContent(_icontent);

            // built-in properties (attributes)
            //_builtInValues1 = new object[]
            //    {
            //        _content.Name,
            //        _content.ParentId,
            //        _content.CreateDate,
            //        _content.UpdateDate,
            //        true, // isDoc
            //        _content.SortOrder,
            //        _content.Level,
            //        _content.TemplateId,
            //        _content.WriterId,
            //        _content.CreatorId,
            //        _content.UrlName,
            //        _content.IsDraft
            //    };

            var i = 0;
            _builtInValues = new []
                {
                    XmlString(i++, _content.Name),
                    XmlString(i++, _content.ParentId),
                    XmlString(i++, _content.CreateDate),
                    XmlString(i++, _content.UpdateDate),
                    XmlString(i++, true), // isDoc
                    XmlString(i++, _content.SortOrder),
                    XmlString(i++, _content.Level),
                    XmlString(i++, _content.TemplateId),
                    XmlString(i++, _content.WriterId),
                    XmlString(i++, _content.CreatorId),
                    XmlString(i++, _content.UrlName),
                    XmlString(i, _content.IsDraft)
                };
        }

        private string XmlString(int index, object value)
        {
            var field = Type.FieldTypes[index];
            return field.XmlStringConverter == null ? value.ToString() : field.XmlStringConverter(value);
        }

        #region INavigableContent

        public IPublishedContent InnerContent
        {
            get { return _icontent; }
        }

        public int Id
        {
            get { return _content.Id; }
        }

        public int ParentId
        {
            get { return _content.ParentId; }
        }

        public INavigableContentType Type
        {
            get { return NavigableContentType.GetContentType(_content.ContentType); }
        }

        // returns all child ids, will be filtered by the source
        public IList<int> ChildIds
        {
            get { return _content.ChildIds; }
        }

        public object Value(int index)
        {
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");

            if (index < NavigableContentType.BuiltinProperties.Length)
            {
                // built-in field, ie attribute
                //return XmlString(index, _builtInValues1[index]);
                return _builtInValues[index];
            }

            index -= NavigableContentType.BuiltinProperties.Length;
            var properties = _content.PropertiesArray;
            if (index >= properties.Length)
                throw new ArgumentOutOfRangeException("index");

            // custom property, ie element
            return properties[index].XPathValue;
        }

        #endregion
    }
}
