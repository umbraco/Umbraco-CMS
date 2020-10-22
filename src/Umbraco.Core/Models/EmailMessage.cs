using System;

namespace Umbraco.Core.Models
{
    public class EmailMessage
    {
        public string From { get; }
        public string To { get; }
        public string Subject { get; }
        public string Body { get; }
        public bool IsBodyHtml { get; set; } = false;

        public EmailMessage(string from, string to, string subject, string body)
        {
            if (string.IsNullOrEmpty(from)) throw new ArgumentException("Value cannot be null or empty.", nameof(from));
            if (string.IsNullOrEmpty(to)) throw new ArgumentException("Value cannot be null or empty.", nameof(to));
            if (string.IsNullOrEmpty(subject)) throw new ArgumentException("Value cannot be null or empty.", nameof(subject));
            if (string.IsNullOrEmpty(body)) throw new ArgumentException("Value cannot be null or empty.", nameof(body));

            From = from;
            To = to;
            Subject = subject;
            Body = body;
        }
    }
}
