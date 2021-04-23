// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Extensions
{
    [TestFixture]
    public class EmailMessageExtensionsTests
    {
        private const string ConfiguredSender = "noreply@umbraco.com";

        [Test]
        public void Can_Construct_MimeMessage_From_Simple_EmailMessage()
        {
            const string from = "from@email.com";
            const string to = "to@email.com";
            const string subject = "Subject";
            const string body = "<p>Message</p>";
            const bool isBodyHtml = true;
            var emailMesasge = new EmailMessage(from, to, subject, body, isBodyHtml);

            var result = emailMesasge.ToMimeMessage(ConfiguredSender);

            Assert.AreEqual(1, result.From.Count());
            Assert.AreEqual(from, result.From.First().ToString());
            Assert.AreEqual(1, result.To.Count());
            Assert.AreEqual(to, result.To.First().ToString());
            Assert.AreEqual(subject, result.Subject);
            Assert.IsNull(result.TextBody);
            Assert.AreEqual(body, result.HtmlBody.ToString());
        }

        [Test]
        public void Can_Construct_MimeMessage_From_Full_EmailMessage()
        {
            const string from = "from@email.com";
            string[] to = new[] { "to@email.com", "to2@email.com" };
            string[] cc = new[] { "cc@email.com", "cc2@email.com" };
            string[] bcc = new[] { "bcc@email.com", "bcc2@email.com", "bcc3@email.com", "invalid@email@address" };
            string[] replyTo = new[] { "replyto@email.com" };
            const string subject = "Subject";
            const string body = "Message";
            const bool isBodyHtml = false;

            using var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
            var attachments = new List<EmailMessageAttachment>
                {
                    new EmailMessageAttachment(attachmentStream, "test.txt"),
                };
            var emailMesasge = new EmailMessage(from, to, cc, bcc, replyTo, subject, body, isBodyHtml, attachments);

            var result = emailMesasge.ToMimeMessage(ConfiguredSender);

            Assert.AreEqual(1, result.From.Count());
            Assert.AreEqual(from, result.From.First().ToString());
            Assert.AreEqual(2, result.To.Count());
            Assert.AreEqual(to[0], result.To.First().ToString());
            Assert.AreEqual(to[1], result.To.Skip(1).First().ToString());
            Assert.AreEqual(2, result.Cc.Count());
            Assert.AreEqual(cc[0], result.Cc.First().ToString());
            Assert.AreEqual(cc[1], result.Cc.Skip(1).First().ToString());
            Assert.AreEqual(3, result.Bcc.Count());
            Assert.AreEqual(bcc[0], result.Bcc.First().ToString());
            Assert.AreEqual(bcc[1], result.Bcc.Skip(1).First().ToString());
            Assert.AreEqual(bcc[2], result.Bcc.Skip(2).First().ToString());
            Assert.AreEqual(1, result.ReplyTo.Count());
            Assert.AreEqual(replyTo[0], result.ReplyTo.First().ToString());
            Assert.AreEqual(subject, result.Subject);
            Assert.IsNull(result.HtmlBody);
            Assert.AreEqual(body, result.TextBody.ToString());
            Assert.AreEqual(1, result.Attachments.Count());
        }
    }
}
