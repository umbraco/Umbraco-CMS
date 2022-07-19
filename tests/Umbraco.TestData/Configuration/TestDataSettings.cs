namespace Umbraco.TestData.Configuration;

public class TestDataSettings
{
    /// <summary>
    ///     Gets or sets a value indicating whether the test data generation is enabled.
    /// </summary>
    public bool Enabled { get; set; } = false;

    /// <summary>
    ///     Gets or sets a value indicating whether persisted local database cache files for content and media are disabled.
    /// </summary>
    /// <value>The URL path.</value>
    public bool IgnoreLocalDb { get; set; } = false;
}
