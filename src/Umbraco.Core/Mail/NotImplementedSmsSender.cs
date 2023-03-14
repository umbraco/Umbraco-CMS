namespace Umbraco.Cms.Core.Mail;

/// <summary>
///     An <see cref="ISmsSender" /> that throws <see cref="NotImplementedException" />
/// </summary>
internal class NotImplementedSmsSender : ISmsSender
{
    public Task SendSmsAsync(string number, string message)
        => throw new NotImplementedException(
            "To send an SMS ensure ISmsSender is implemented with a custom implementation");
}
