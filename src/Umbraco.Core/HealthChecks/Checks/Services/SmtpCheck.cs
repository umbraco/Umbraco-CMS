// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Net.Sockets;
using Microsoft.Extensions.Options;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Services;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.HealthChecks.Checks.Services;

/// <summary>
///     Health check for the recommended setup regarding SMTP.
/// </summary>
[HealthCheck(
    "1B5D221B-CE99-4193-97CB-5F3261EC73DF",
    "SMTP Settings",
    Description = "Checks that valid settings for sending emails are in place.",
    Group = "Services")]
public class SmtpCheck : HealthCheck
{
    private readonly IOptionsMonitor<GlobalSettings> _globalSettings;
    private readonly ILocalizedTextService _textService;

    /// <summary>
    ///     Initializes a new instance of the <see cref="SmtpCheck" /> class.
    /// </summary>
    public SmtpCheck(ILocalizedTextService textService, IOptionsMonitor<GlobalSettings> globalSettings)
    {
        _textService = textService;
        _globalSettings = globalSettings;
    }

    /// <summary>
    ///     Get the status for this health check
    /// </summary>
    public override Task<IEnumerable<HealthCheckStatus>> GetStatus() =>
        Task.FromResult(CheckSmtpSettings().Yield());

    /// <summary>
    ///     Executes the action and returns it's status
    /// </summary>
    public override HealthCheckStatus ExecuteAction(HealthCheckAction action)
        => throw new InvalidOperationException("SmtpCheck has no executable actions");

    private static bool CanMakeSmtpConnection(string host, int port)
    {
        try
        {
            using (var client = new TcpClient())
            {
                client.Connect(host, port);
                using (NetworkStream stream = client.GetStream())
                {
                    using (var writer = new StreamWriter(stream))
                    using (var reader = new StreamReader(stream))
                    {
                        writer.WriteLine("EHLO " + host);
                        writer.Flush();
                        reader.ReadLine();
                        return true;
                    }
                }
            }
        }
        catch
        {
            return false;
        }
    }

    private HealthCheckStatus CheckSmtpSettings()
    {
        var success = false;

        SmtpSettings? smtpSettings = _globalSettings.CurrentValue.Smtp;

        string message;
        if (smtpSettings == null)
        {
            message = _textService.Localize("healthcheck", "smtpMailSettingsNotFound");
        }
        else
        {
            if (string.IsNullOrEmpty(smtpSettings.Host))
            {
                message = _textService.Localize("healthcheck", "smtpMailSettingsHostNotConfigured");
            }
            else
            {
                success = CanMakeSmtpConnection(smtpSettings.Host, smtpSettings.Port);
                message = success
                    ? _textService.Localize("healthcheck", "smtpMailSettingsConnectionSuccess")
                    : _textService.Localize(
                        "healthcheck", "smtpMailSettingsConnectionFail", new[] { smtpSettings.Host, smtpSettings.Port.ToString() });
            }
        }

        return
            new HealthCheckStatus(message)
            {
                ResultType = success ? StatusResultType.Success : StatusResultType.Error,
                ReadMoreLink = success ? null : Constants.HealthChecks.DocumentationLinks.SmtpCheck,
            };
    }
}
