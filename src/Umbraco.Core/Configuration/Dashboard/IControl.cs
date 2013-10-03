namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IControl
    {
        bool ShowOnce { get; }

        bool AddPanel { get; }

        string PanelCaption { get; }

        string ControlPath { get; }
    }
}