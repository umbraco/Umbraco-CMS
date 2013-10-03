namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IDashboardControl
    {
        bool ShowOnce { get; }

        bool AddPanel { get; }

        string PanelCaption { get; }

        string ControlPath { get; }

        IAccess AccessRights { get; }
    }
}