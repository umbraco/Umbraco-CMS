using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Umbraco.Cms.Infrastructure.Mail.Interfaces;

namespace Umbraco.Cms.Infrastructure.Mail;

/// <summary>
///     Delivers the emails held by the <see cref="InMemoryEmailDeliveryQueue"/> through the <see cref="IEmailSenderClient"/>.
/// </summary>
/// <remarks>
///     <para>
///         Does nothing when <see cref="IEmailDeliveryQueue"/> has been replaced, as the replacement owns its delivery.
///     </para>
///     <para>
///         Unlike recurring background jobs, this deliberately runs on every instance, whatever its server role or runtime
///         level. The queue is held in memory, so each instance only holds the emails its own requests queued and must
///         deliver them itself; restricting delivery to a server role would leave emails queued on other instances
///         undelivered. Delivery does not use the database, so it does not need to wait for the runtime to be running.
///     </para>
/// </remarks>
internal sealed class EmailDeliveryHostedService : BackgroundService
{
    private readonly InMemoryEmailDeliveryQueue? _queue;
    private readonly IEmailSenderClient _emailSenderClient;
    private readonly ILogger<EmailDeliveryHostedService> _logger;

    /// <summary>
    ///     Initializes a new instance of the <see cref="EmailDeliveryHostedService"/> class.
    /// </summary>
    /// <param name="queue">The queue holding the emails to deliver.</param>
    /// <param name="emailSenderClient">The client used to deliver the emails.</param>
    /// <param name="logger">The logger.</param>
    public EmailDeliveryHostedService(
        IEmailDeliveryQueue queue,
        IEmailSenderClient emailSenderClient,
        ILogger<EmailDeliveryHostedService> logger)
    {
        _queue = queue as InMemoryEmailDeliveryQueue;
        _emailSenderClient = emailSenderClient;
        _logger = logger;
    }

    /// <inheritdoc />
    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        await base.StopAsync(cancellationToken);

        if (_queue is null)
        {
            return;
        }

        // Deliver what is still queued for as long as the host allows, as anything left is lost when the application stops.
        while (cancellationToken.IsCancellationRequested is false && _queue.Reader.TryRead(out InMemoryEmailDeliveryQueue.QueuedEmail? email))
        {
            await DeliverAsync(email);
        }

        if (_queue.Reader.Count > 0)
        {
            _logger.LogWarning("The application stopped before {Count} queued emails could be sent.", _queue.Reader.Count);
        }
    }

    /// <inheritdoc />
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (_queue is null)
        {
            return;
        }

        try
        {
            await foreach (InMemoryEmailDeliveryQueue.QueuedEmail email in _queue.Reader.ReadAllAsync(stoppingToken))
            {
                await DeliverAsync(email);
            }
        }
        catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested)
        {
            // The application is stopping; StopAsync delivers what is still queued.
        }
    }

    private async Task DeliverAsync(InMemoryEmailDeliveryQueue.QueuedEmail email)
    {
        try
        {
            await _emailSenderClient.SendAsync(email.Message, email.Expires);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Could not send a queued {EmailType} email.", email.EmailType);
        }
        finally
        {
            _queue!.Release(email);
        }
    }
}
