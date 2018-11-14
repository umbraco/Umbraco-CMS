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
    }
}
