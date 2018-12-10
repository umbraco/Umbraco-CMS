namespace Umbraco.Core.Configuration.Dashboard
{
    public interface IDashboardControl
    {
        string PanelCaption { get; }

        string ControlPath { get; }

        IAccess AccessRights { get; }
    }
}
