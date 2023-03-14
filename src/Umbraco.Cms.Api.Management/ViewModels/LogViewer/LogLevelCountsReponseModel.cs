namespace Umbraco.Cms.Api.Management.ViewModels.LogViewer;

public class LogLevelCountsReponseModel
{
    /// <summary>
    ///     Gets or sets the Information level count.
    /// </summary>
    public int Information { get; set; }

    /// <summary>
    ///     Gets or sets the Debug level count.
    /// </summary>
    public int Debug { get; set; }

    /// <summary>
    ///     Gets or sets the Warning level count.
    /// </summary>
    public int Warning { get; set; }

    /// <summary>
    ///     Gets or sets the Error level count.
    /// </summary>
    public int Error { get; set; }

    /// <summary>
    ///     Gets or sets the Fatal level count.
    /// </summary>
    public int Fatal { get; set; }
}
