namespace Umbraco.Cms.Core.Models.DeliveryApi;

/// <summary>
///     Represents a block item in the Delivery API.
/// </summary>
public class ApiBlockItem
{
    /// <summary>
    ///     Initializes a new instance of the <see cref="ApiBlockItem" /> class.
    /// </summary>
    /// <param name="content">The content element of the block.</param>
    /// <param name="settings">The optional settings element of the block.</param>
    public ApiBlockItem(IApiElement content, IApiElement? settings)
    {
        Content = content;
        Settings = settings;
    }

    /// <summary>
    ///     Gets the content element of the block.
    /// </summary>
    public IApiElement Content { get; }

    /// <summary>
    ///     Gets the optional settings element of the block.
    /// </summary>
    public IApiElement? Settings { get; }
}
