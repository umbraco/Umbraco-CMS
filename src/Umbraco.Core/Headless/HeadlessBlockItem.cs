namespace Umbraco.Cms.Core.Headless;

public class HeadlessBlockItem
{
    public HeadlessBlockItem(IHeadlessElement content, IHeadlessElement? settings)
    {
        Content = content;
        Settings = settings;
    }

    public IHeadlessElement Content { get; }

    public IHeadlessElement? Settings { get; }
}
