namespace Umbraco.Core.Models
{
    public class NotificationData
    {
        public NotificationData(IContent content, IContentType contentType, ITemplate template)
        {
            Content = content;
            ContentType = contentType;
            Template = template;
        }

        public IContent Content { get;  }
        public IContentType ContentType { get;  }
        public ITemplate Template { get;  }

        protected bool Equals(NotificationData other)
        {
            return Equals(Content, other.Content) && Equals(ContentType, other.ContentType) && Equals(Template, other.Template);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((NotificationData) obj);
        }

        public override int GetHashCode()
        {
            unchecked
            {
                var hashCode = (Content != null ? Content.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (ContentType != null ? ContentType.GetHashCode() : 0);
                hashCode = (hashCode * 397) ^ (Template != null ? Template.GetHashCode() : 0);
                return hashCode;
            }
        }
    }
}
