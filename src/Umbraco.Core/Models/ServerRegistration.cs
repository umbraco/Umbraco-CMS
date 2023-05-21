using System.Globalization;
using Umbraco.Cms.Core.Models.Entities;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Models;

/// <summary>
///     Represents a registered server in a multiple-servers environment.
/// </summary>
public class ServerRegistration : EntityBase, IServerRegistration
{
    private bool _isActive;
    private bool _isSchedulingPublisher;
    private string? _serverAddress;
    private string? _serverIdentity;

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerRegistration" /> class.
    /// </summary>
    public ServerRegistration()
    {
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerRegistration" /> class.
    /// </summary>
    /// <param name="id">The unique id of the server registration.</param>
    /// <param name="serverAddress">The server URL.</param>
    /// <param name="serverIdentity">The unique server identity.</param>
    /// <param name="registered">The date and time the registration was created.</param>
    /// <param name="accessed">The date and time the registration was last accessed.</param>
    /// <param name="isActive">A value indicating whether the registration is active.</param>
    /// <param name="isSchedulingPublisher">A value indicating whether the registration is scheduling publisher.</param>
    public ServerRegistration(int id, string? serverAddress, string? serverIdentity, DateTime registered, DateTime accessed, bool isActive, bool isSchedulingPublisher)
    {
        UpdateDate = accessed;
        CreateDate = registered;
        Key = id.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
        Id = id;
        ServerAddress = serverAddress;
        ServerIdentity = serverIdentity;
        IsActive = isActive;
        IsSchedulingPublisher = isSchedulingPublisher;
    }

    /// <summary>
    ///     Initializes a new instance of the <see cref="ServerRegistration" /> class.
    /// </summary>
    /// <param name="serverAddress">The server URL.</param>
    /// <param name="serverIdentity">The unique server identity.</param>
    /// <param name="registered">The date and time the registration was created.</param>
    public ServerRegistration(string serverAddress, string serverIdentity, DateTime registered)
    {
        CreateDate = registered;
        UpdateDate = registered;
        Key = 0.ToString(CultureInfo.InvariantCulture).EncodeAsGuid();
        ServerAddress = serverAddress;
        ServerIdentity = serverIdentity;
    }

    /// <summary>
    ///     Gets or sets the server URL.
    /// </summary>
    public string? ServerAddress
    {
        get => _serverAddress;
        set => SetPropertyValueAndDetectChanges(value, ref _serverAddress, nameof(ServerAddress));
    }

    /// <summary>
    ///     Gets or sets the server unique identity.
    /// </summary>
    public string? ServerIdentity
    {
        get => _serverIdentity;
        set => SetPropertyValueAndDetectChanges(value, ref _serverIdentity, nameof(ServerIdentity));
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the server is active.
    /// </summary>
    public bool IsActive
    {
        get => _isActive;
        set => SetPropertyValueAndDetectChanges(value, ref _isActive, nameof(IsActive));
    }

    /// <summary>
    ///     Gets or sets a value indicating whether the server has the SchedulingPublisher role
    /// </summary>
    public bool IsSchedulingPublisher
    {
        get => _isSchedulingPublisher;
        set => SetPropertyValueAndDetectChanges(value, ref _isSchedulingPublisher, nameof(IsSchedulingPublisher));
    }

    /// <summary>
    ///     Gets the date and time the registration was created.
    /// </summary>
    public DateTime Registered
    {
        get => CreateDate;
        set => CreateDate = value;
    }

    /// <summary>
    ///     Gets the date and time the registration was last accessed.
    /// </summary>
    public DateTime Accessed
    {
        get => UpdateDate;
        set => UpdateDate = value;
    }

    /// <summary>
    ///     Converts the value of this instance to its equivalent string representation.
    /// </summary>
    /// <returns></returns>
    public override string ToString() => string.Format("{{\"{0}\", \"{1}\", {2}active, {3}master}}", ServerAddress, ServerIdentity, IsActive ? string.Empty : "!", IsSchedulingPublisher ? string.Empty : "!");
}
