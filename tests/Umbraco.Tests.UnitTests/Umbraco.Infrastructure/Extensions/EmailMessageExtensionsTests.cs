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

        Assert.That(result.From.Count(), Is.EqualTo(1));
        Assert.That(result.From.First().ToString(), Is.EqualTo(from));
        Assert.That(result.To.Count(), Is.EqualTo(1));
        Assert.That(result.To.First().ToString(), Is.EqualTo(to));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.TextBody, Is.Null);
        Assert.That(result.HtmlBody, Is.EqualTo(body));
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

        Assert.That(result.From.Count(), Is.EqualTo(1));
        Assert.That(result.From.First().ToString(), Is.EqualTo(from));
        Assert.That(result.To.Count(), Is.EqualTo(2));
        Assert.That(result.To.First().ToString(), Is.EqualTo(to[0]));
        Assert.That(result.To.Skip(1).First().ToString(), Is.EqualTo(to[1]));
        Assert.That(result.Cc.Count(), Is.EqualTo(2));
        Assert.That(result.Cc.First().ToString(), Is.EqualTo(cc[0]));
        Assert.That(result.Cc.Skip(1).First().ToString(), Is.EqualTo(cc[1]));
        Assert.That(result.Bcc.Count(), Is.EqualTo(3));
        Assert.That(result.Bcc.First().ToString(), Is.EqualTo(bcc[0]));
        Assert.That(result.Bcc.Skip(1).First().ToString(), Is.EqualTo(bcc[1]));
        Assert.That(result.Bcc.Skip(2).First().ToString(), Is.EqualTo(bcc[2]));
        Assert.That(result.ReplyTo.Count(), Is.EqualTo(1));
        Assert.That(result.ReplyTo.First().ToString(), Is.EqualTo(replyTo[0]));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.HtmlBody, Is.Null);
        Assert.That(result.TextBody, Is.EqualTo(body));
        Assert.That(result.Attachments.Count(), Is.EqualTo(1));
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

        Assert.That(result.From.Count(), Is.EqualTo(1));
        Assert.That(result.From.First().ToString(), Is.EqualTo(ConfiguredSender));
        Assert.That(result.To.Count(), Is.EqualTo(1));
        Assert.That(result.To.First().ToString(), Is.EqualTo(to));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.TextBody, Is.Null);
        Assert.That(result.HtmlBody, Is.EqualTo(body));
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

        Assert.That(result.From.Address, Is.EqualTo(from));
        Assert.That(result.From.DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.To.Count(), Is.EqualTo(1));
        Assert.That(result.To.First().Address, Is.EqualTo(to));
        Assert.That(result.To.First().DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.Body, Is.EqualTo(body));
        Assert.That(result.IsBodyHtml, Is.True);
        Assert.That(result.HasAttachments, Is.False);
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

        Assert.That(result.From.Address, Is.EqualTo(ConfiguredSender));
        Assert.That(result.From.DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.To.Count(), Is.EqualTo(1));
        Assert.That(result.To.First().Address, Is.EqualTo(to));
        Assert.That(result.To.First().DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.Body, Is.EqualTo(body));
        Assert.That(result.IsBodyHtml, Is.True);
        Assert.That(result.HasAttachments, Is.False);
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

        Assert.That(result.From.Address, Is.EqualTo("from@from.com"));
        Assert.That(result.From.DisplayName, Is.EqualTo("From Email"));
        Assert.That(result.To.Count(), Is.EqualTo(1));
        Assert.That(result.To.First().Address, Is.EqualTo("to@to.com"));
        Assert.That(result.To.First().DisplayName, Is.EqualTo("To Email"));
        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.Body, Is.EqualTo(body));
        Assert.That(result.IsBodyHtml, Is.True);
        Assert.That(result.HasAttachments, Is.False);
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

        Assert.That(result.From.Address, Is.EqualTo("from@from.com"));
        Assert.That(result.From.DisplayName, Is.EqualTo("From Email"));

        Assert.That(result.To.Count(), Is.EqualTo(2));
        Assert.That(result.To.First().Address, Is.EqualTo("to@email.com"));
        Assert.That(result.To.First().DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.To.Skip(1).First().Address, Is.EqualTo("to2@email.com"));
        Assert.That(result.To.Skip(1).First().DisplayName, Is.EqualTo("Second Email"));

        Assert.That(result.Cc.Count(), Is.EqualTo(2));
        Assert.That(result.Cc.First().Address, Is.EqualTo("cc@email.com"));
        Assert.That(result.Cc.First().DisplayName, Is.EqualTo("First CC"));
        Assert.That(result.Cc.Skip(1).First().Address, Is.EqualTo("cc2@email.com"));
        Assert.That(result.Cc.Skip(1).First().DisplayName, Is.EqualTo(string.Empty));

        Assert.That(result.Bcc.Count(), Is.EqualTo(3));
        Assert.That(result.Bcc.First().Address, Is.EqualTo("bcc@email.com"));
        Assert.That(result.Bcc.First().DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.Bcc.Skip(1).First().Address, Is.EqualTo("bcc2@email.com"));
        Assert.That(result.Bcc.Skip(1).First().DisplayName, Is.EqualTo(string.Empty));
        Assert.That(result.Bcc.Skip(2).First().Address, Is.EqualTo("bcc3@email.com"));
        Assert.That(result.Bcc.Skip(2).First().DisplayName, Is.EqualTo("Third BCC"));

        Assert.That(result.ReplyTo.Count(), Is.EqualTo(1));
        Assert.That(result.ReplyTo.First().Address, Is.EqualTo("replyto@email.com"));
        Assert.That(result.ReplyTo.First().DisplayName, Is.EqualTo(string.Empty));

        Assert.That(result.Subject, Is.EqualTo(subject));
        Assert.That(result.Body, Is.EqualTo(body));
        Assert.That(result.Attachments.Count(), Is.EqualTo(1));
    }
}
