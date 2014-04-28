using System;
using System.Xml.Linq;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Used in content/media/member repositories in order to add this type of entity to the persisted collection to be saved
    /// in a single transaction during saving an entity
    /// </summary>
    internal class ContentXmlEntity : IAggregateRoot
    {
        private readonly bool _entityExists;
        private readonly Func<XElement> _xml;

        public ContentXmlEntity(bool entityExists, IContentBase content, Func<XElement> xml)
        {            
            if (content == null) throw new ArgumentNullException("content");
            _entityExists = entityExists;
            _xml = xml;            
            Content = content;
        }

        public XElement Xml
        {
            get { return _xml(); }
        }
        public IContentBase Content { get; private set; }

        public int Id
        {
            get { return Content.Id; }
            set { throw new NotSupportedException(); }
        }

        public Guid Key { get; set; }
        public DateTime CreateDate { get; set; }
        public DateTime UpdateDate { get; set; }

        public bool HasIdentity
        {
            get { return _entityExists; }         
        }

        public object DeepClone()
        {
            var clone = (ContentXmlEntity)MemberwiseClone();
            //Automatically deep clone ref properties that are IDeepCloneable
            DeepCloneHelper.DeepCloneRefProperties(this, clone);
            return clone;
        }
    }
}