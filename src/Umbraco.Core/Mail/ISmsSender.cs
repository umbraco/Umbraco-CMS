namespace Umbraco.Cms.Core.Mail;

/// <summary>
/// Service to send an SMS.
/// </summary>
/// <remarks>
/// Borrowed from ASP.NET Core documentation samples.
/// </remarks>
public interface ISmsSender
{
    /// <summary>
    /// Sends an SMS message asynchronously.
    /// </summary>
    /// <param name="number">The phone number to send the SMS to.</param>
    /// <param name="message">The message content to send.</param>
    /// <returns>A task representing the asynchronous operation.</returns>
    Task SendSmsAsync(string number, string message);
}
