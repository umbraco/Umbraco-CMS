// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Umbraco.Cms.Core.Models.Email;
using Umbraco.Cms.Infrastructure.Extensions;

namespace Umbraco.Cms.Tests.UnitTests.Umbraco.Infrastructure.Extensions;

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
        var emailMessage = new EmailMessage(from, to, subject, body, isBodyHtml);

        var result = emailMessage.ToMimeMessage(ConfiguredSender);

        Assert.AreEqual(1, result.From.Count());
        Assert.AreEqual(from, result.From.First().ToString());
        Assert.AreEqual(1, result.To.Count());
        Assert.AreEqual(to, result.To.First().ToString());
        Assert.AreEqual(subject, result.Subject);
        Assert.IsNull(result.TextBody);
        Assert.AreEqual(body, result.HtmlBody);
    }

    [Test]
    public void Can_Construct_MimeMessage_From_Full_EmailMessage()
    {
        const string from = "from@email.com";
        string[] to = { "to@email.com", "to2@email.com" };
        string[] cc = { "cc@email.com", "cc2@email.com" };
        string[] bcc = { "bcc@email.com", "bcc2@email.com", "bcc3@email.com", "invalid@email@address" };
        string[] replyTo = { "replyto@email.com" };
        const string subject = "Subject";
        const string body = "Message";
        const bool isBodyHtml = false;

        using var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var attachments = new List<EmailMessageAttachment> { new(attachmentStream, "test.txt") };
        var emailMessage = new EmailMessage(from, to, cc, bcc, replyTo, subject, body, isBodyHtml, attachments);

        var result = emailMessage.ToMimeMessage(ConfiguredSender);

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
        Assert.AreEqual(body, result.TextBody);
        Assert.AreEqual(1, result.Attachments.Count());
    }

    [Test]
    public void Can_Construct_MimeMessage_With_ConfiguredSender()
    {
        const string to = "to@email.com";
        const string subject = "Subject";
        const string body = "<p>Message</p>";
        const bool isBodyHtml = true;
        var emailMessage = new EmailMessage(null, to, subject, body, isBodyHtml);

        var result = emailMessage.ToMimeMessage(ConfiguredSender);

        Assert.AreEqual(1, result.From.Count());
        Assert.AreEqual(ConfiguredSender, result.From.First().ToString());
        Assert.AreEqual(1, result.To.Count());
        Assert.AreEqual(to, result.To.First().ToString());
        Assert.AreEqual(subject, result.Subject);
        Assert.IsNull(result.TextBody);
        Assert.AreEqual(body, result.HtmlBody);
    }

    [Test]
    public void Can_Construct_NotificationEmailModel_From_Simple_MailMessage()
    {
        const string from = "from@email.com";
        const string to = "to@email.com";
        const string subject = "Subject";
        const string body = "<p>Message</p>";
        const bool isBodyHtml = true;
        var emailMessage = new EmailMessage(from, to, subject, body, isBodyHtml);

        var result = emailMessage.ToNotificationEmail(ConfiguredSender);

        Assert.AreEqual(from, result.From.Address);
        Assert.AreEqual(string.Empty, result.From.DisplayName);
        Assert.AreEqual(1, result.To.Count());
        Assert.AreEqual(to, result.To.First().Address);
        Assert.AreEqual(string.Empty, result.To.First().DisplayName);
        Assert.AreEqual(subject, result.Subject);
        Assert.AreEqual(body, result.Body);
        Assert.IsTrue(result.IsBodyHtml);
        Assert.IsFalse(result.HasAttachments);
    }

    [Test]
    public void Can_Construct_NotificationEmailModel_From_Simple_MailMessage_With_Configured_Sender()
    {
        const string to = "to@email.com";
        const string subject = "Subject";
        const string body = "<p>Message</p>";
        const bool isBodyHtml = true;
        var emailMessage = new EmailMessage(null, to, subject, body, isBodyHtml);

        var result = emailMessage.ToNotificationEmail(ConfiguredSender);

        Assert.AreEqual(ConfiguredSender, result.From.Address);
        Assert.AreEqual(string.Empty, result.From.DisplayName);
        Assert.AreEqual(1, result.To.Count());
        Assert.AreEqual(to, result.To.First().Address);
        Assert.AreEqual(string.Empty, result.To.First().DisplayName);
        Assert.AreEqual(subject, result.Subject);
        Assert.AreEqual(body, result.Body);
        Assert.IsTrue(result.IsBodyHtml);
        Assert.IsFalse(result.HasAttachments);
    }

    [Test]
    public void Can_Construct_NotificationEmailModel_From_Simple_MailMessage_With_DisplayName()
    {
        const string from = "\"From Email\" <from@from.com>";
        const string to = "\"To Email\" <to@to.com>";
        const string subject = "Subject";
        const string body = "<p>Message</p>";
        const bool isBodyHtml = true;
        var emailMessage = new EmailMessage(from, to, subject, body, isBodyHtml);

        var result = emailMessage.ToNotificationEmail(ConfiguredSender);

        Assert.AreEqual("from@from.com", result.From.Address);
        Assert.AreEqual("From Email", result.From.DisplayName);
        Assert.AreEqual(1, result.To.Count());
        Assert.AreEqual("to@to.com", result.To.First().Address);
        Assert.AreEqual("To Email", result.To.First().DisplayName);
        Assert.AreEqual(subject, result.Subject);
        Assert.AreEqual(body, result.Body);
        Assert.IsTrue(result.IsBodyHtml);
        Assert.IsFalse(result.HasAttachments);
    }

    [Test]
    public void Can_Construct_NotificationEmailModel_From_Full_EmailMessage()
    {
        const string from = "\"From Email\" <from@from.com>";
        string[] to = { "to@email.com", "\"Second Email\" <to2@email.com>", "invalid@invalid@invalid" };
        string[] cc = { "\"First CC\" <cc@email.com>", "cc2@email.com", "invalid@invalid@invalid" };
        string[] bcc = { "bcc@email.com", "bcc2@email.com", "\"Third BCC\" <bcc3@email.com>", "invalid@email@address" };
        string[] replyTo = { "replyto@email.com", "invalid@invalid@invalid" };
        const string subject = "Subject";
        const string body = "Message";
        const bool isBodyHtml = false;

        using var attachmentStream = new MemoryStream(Encoding.UTF8.GetBytes("test"));
        var attachments = new List<EmailMessageAttachment> { new(attachmentStream, "test.txt") };
        var emailMessage = new EmailMessage(from, to, cc, bcc, replyTo, subject, body, isBodyHtml, attachments);

        var result = emailMessage.ToNotificationEmail(ConfiguredSender);

        Assert.AreEqual("from@from.com", result.From.Address);
        Assert.AreEqual("From Email", result.From.DisplayName);

        Assert.AreEqual(2, result.To.Count());
        Assert.AreEqual("to@email.com", result.To.First().Address);
        Assert.AreEqual(string.Empty, result.To.First().DisplayName);
        Assert.AreEqual("to2@email.com", result.To.Skip(1).First().Address);
        Assert.AreEqual("Second Email", result.To.Skip(1).First().DisplayName);

        Assert.AreEqual(2, result.Cc.Count());
        Assert.AreEqual("cc@email.com", result.Cc.First().Address);
        Assert.AreEqual("First CC", result.Cc.First().DisplayName);
        Assert.AreEqual("cc2@email.com", result.Cc.Skip(1).First().Address);
        Assert.AreEqual(string.Empty, result.Cc.Skip(1).First().DisplayName);

        Assert.AreEqual(3, result.Bcc.Count());
        Assert.AreEqual("bcc@email.com", result.Bcc.First().Address);
        Assert.AreEqual(string.Empty, result.Bcc.First().DisplayName);
        Assert.AreEqual("bcc2@email.com", result.Bcc.Skip(1).First().Address);
        Assert.AreEqual(string.Empty, result.Bcc.Skip(1).First().DisplayName);
        Assert.AreEqual("bcc3@email.com", result.Bcc.Skip(2).First().Address);
        Assert.AreEqual("Third BCC", result.Bcc.Skip(2).First().DisplayName);

        Assert.AreEqual(1, result.ReplyTo.Count());
        Assert.AreEqual("replyto@email.com", result.ReplyTo.First().Address);
        Assert.AreEqual(string.Empty, result.ReplyTo.First().DisplayName);

        Assert.AreEqual(subject, result.Subject);
        Assert.AreEqual(body, result.Body);
        Assert.AreEqual(1, result.Attachments.Count());
    }
}
