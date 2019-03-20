﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.XPath;
using Umbraco.Core.Models.PublishedContent;
using Umbraco.Core.Xml;

namespace Umbraco.Web.PublishedCache
{
    abstract class PublishedCacheBase : IPublishedCache
    {
        public bool PreviewDefault { get; }

        protected PublishedCacheBase(bool previewDefault)
        {
            PreviewDefault = previewDefault;
        }

        public abstract IPublishedContent GetById(bool preview, int contentId);

        public IPublishedContent GetById(int contentId)
        {
            return GetById(PreviewDefault, contentId);
        }

        public abstract IPublishedContent GetById(bool preview, Guid contentId);

        public IPublishedContent GetById(Guid contentId)
        {
            return GetById(PreviewDefault, contentId);
        }

        public abstract bool HasById(bool preview, int contentId);

        public bool HasById(int contentId)
        {
            return HasById(PreviewDefault, contentId);
        }

        public abstract IEnumerable<IPublishedContent> GetAtRoot(bool preview);

        public IEnumerable<IPublishedContent> GetAtRoot()
        {
            return GetAtRoot(PreviewDefault);
        }

        public abstract IPublishedContent GetSingleByXPath(bool preview, string xpath, XPathVariable[] vars);

        public IPublishedContent GetSingleByXPath(string xpath, XPathVariable[] vars)
        {
            return GetSingleByXPath(PreviewDefault, xpath, vars);
        }

        public abstract IPublishedContent GetSingleByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars);

        public IPublishedContent GetSingleByXPath(XPathExpression xpath, XPathVariable[] vars)
        {
            return GetSingleByXPath(PreviewDefault, xpath, vars);
        }

        public abstract IEnumerable<IPublishedContent> GetByXPath(bool preview, string xpath, XPathVariable[] vars);

        public IEnumerable<IPublishedContent> GetByXPath(string xpath, XPathVariable[] vars)
        {
            return GetByXPath(PreviewDefault, xpath, vars);
        }

        public abstract IEnumerable<IPublishedContent> GetByXPath(bool preview, XPathExpression xpath, XPathVariable[] vars);

        public IEnumerable<IPublishedContent> GetByXPath(XPathExpression xpath, XPathVariable[] vars)
        {
            return GetByXPath(PreviewDefault, xpath, vars);
        }

        public abstract XPathNavigator CreateNavigator(bool preview);

        public XPathNavigator CreateNavigator()
        {
            return CreateNavigator(PreviewDefault);
        }

        public abstract XPathNavigator CreateNodeNavigator(int id, bool preview);

        public abstract bool HasContent(bool preview);

        public bool HasContent()
        {
            return HasContent(PreviewDefault);
        }

        public abstract PublishedContentType GetContentType(int id);

        public abstract PublishedContentType GetContentType(string alias);

        public virtual IEnumerable<IPublishedContent> GetByContentType(PublishedContentType contentType)
        {
            // this is probably not super-efficient, but works
            // some cache implementation may want to override it, though
            return GetAtRoot()
                .SelectMany(x => x.DescendantsOrSelf())
                .Where(x => x.ContentType.Id == contentType.Id);
        }
    }
}
