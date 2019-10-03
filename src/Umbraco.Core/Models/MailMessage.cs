namespace Umbraco.Core.Models
{
    public class MailMessage
    {
        public MailAddress From { get; set; }
        public MailAddress To { get; set; }
        public string Body { get; set; }
        public string Subject { get; set; }
        public bool IsBodyHtml { get; set; }
    }
}
