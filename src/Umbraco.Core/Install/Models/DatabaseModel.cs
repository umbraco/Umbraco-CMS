// Copyright (c) Umbraco.
// See LICENSE for more details.

using System.Runtime.Serialization;

namespace Umbraco.Cms.Core.Install.Models;

/// <summary>
///     Represents the database configuration model used during installation.
/// </summary>
[DataContract(Name = "database", Namespace = "")]
public class DatabaseModel
{
    /// <summary>
    ///     Gets or sets the unique identifier for the database provider metadata.
    /// </summary>
    [DataMember(Name = "databaseProviderMetadataId")]
    public Guid DatabaseProviderMetadataId { get; set; }

    /// <summary>
    ///     Gets or sets the name of the database provider.
    /// </summary>
    [DataMember(Name = "providerName")]
    public string? ProviderName { get; set; }

    // TODO: Make this nullable in V11
    // Server can be null, for instance when installing a SQLite database.
    /// <summary>
    ///     Gets or sets the database server address.
    /// </summary>
    /// <remarks>
    ///     This value can be null for certain database providers, such as SQLite.
    /// </remarks>
    [DataMember(Name = "server")]
    public string Server { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the name of the database.
    /// </summary>
    [DataMember(Name = "databaseName")]
    public string DatabaseName { get; set; } = null!;

    /// <summary>
    ///     Gets or sets the login username for database authentication.
    /// </summary>
    [DataMember(Name = "login")]
    public string? Login { get; set; }

    /// <summary>
    ///     Gets or sets the password for database authentication.
    /// </summary>
    [DataMember(Name = "password")]
    public string? Password { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to use integrated (Windows) authentication.
    /// </summary>
    [DataMember(Name = "integratedAuth")]
    public bool IntegratedAuth { get; set; }

    /// <summary>
    ///     Gets or sets a value indicating whether to trust the server certificate.
    /// </summary>
    [DataMember(Name = "trustServerCertificate")]
    public bool TrustServerCertificate { get; set; }

    /// <summary>
    ///     Gets or sets the full connection string for the database.
    /// </summary>
    /// <remarks>
    ///     When provided, this connection string is used directly instead of building one from the other properties.
    /// </remarks>
    [DataMember(Name = "connectionString")]
    public string? ConnectionString { get; set; }
}
