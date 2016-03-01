using System;
using System.Xml.Linq;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Used content repository in order to add an entity to the persisted collection to be saved
    /// in a single transaction during saving an entity
    /// </summary>
    internal class ContentPreviewEntity<TContent> : ContentXmlEntity<TContent> 
        where TContent : IContentBase
    {
        public ContentPreviewEntity(TContent content, Func<TContent, XElement> xml)
            : base(content, xml)
        {
        }

        public Guid Version
        {
            get { return Content.Version; }
        }
    }
}