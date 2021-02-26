using System;

namespace Umbraco.Cms.Core.Models
{
    public class EmailMessage
    {
        public string From { get; }
        public string To { get; }
        public string Subject { get; }
        public string Body { get; }
        public bool IsBodyHtml { get; }

        public EmailMessage(string from, string to, string subject, string body, bool isBodyHtml)
        {
            if (from == null) throw new ArgumentNullException(nameof(from));
            if (from.Length == 0) throw new ArgumentException("Value cannot be empty.", nameof(from));

            if (to == null) throw new ArgumentNullException(nameof(to));
            if (to.Length == 0) throw new ArgumentException("Value cannot be empty.", nameof(to));

            if (subject == null) throw new ArgumentNullException(nameof(subject));
            if (subject.Length == 0) throw new ArgumentException("Value cannot be empty.", nameof(subject));

            if (body == null) throw new ArgumentNullException(nameof(body));
            if (body.Length == 0) throw new ArgumentException("Value cannot be empty.", nameof(body));

            From = from;
            To = to;
            Subject = subject;
            Body = body;

            IsBodyHtml = isBodyHtml;
        }
    }
}
