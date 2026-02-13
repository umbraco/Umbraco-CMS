namespace Umbraco.Cms.Core.Mail;

/// <summary>
/// An <see cref="ISmsSender"/> that throws <see cref="NotImplementedException"/>.
/// </summary>
/// <remarks>
/// This is the default implementation used when no custom SMS sender is configured.
/// To send SMS messages, implement <see cref="ISmsSender"/> with a custom implementation.
/// </remarks>
internal sealed class NotImplementedSmsSender : ISmsSender
{
    /// <inheritdoc />
    public Task SendSmsAsync(string number, string message)
        => throw new NotImplementedException(
            "To send an SMS ensure ISmsSender is implemented with a custom implementation");
}
