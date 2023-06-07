namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("Will no longer be required with the use of IInstallStep in the new backoffice API")]
public sealed class InstallSetupStepAttribute : Attribute
{
    public InstallSetupStepAttribute(InstallationType installTypeTarget, string name, string view, int serverOrder, string description)
    {
        InstallTypeTarget = installTypeTarget;
        Name = name;
        View = view;
        ServerOrder = serverOrder;
        Description = description;

        // default
        PerformsAppRestart = false;
    }

    public InstallSetupStepAttribute(InstallationType installTypeTarget, string name, int serverOrder, string description)
    {
        InstallTypeTarget = installTypeTarget;
        Name = name;
        View = string.Empty;
        ServerOrder = serverOrder;
        Description = description;

        // default
        PerformsAppRestart = false;
    }

    public InstallationType InstallTypeTarget { get; }

    public string Name { get; }

    public string View { get; }

    public int ServerOrder { get; }

    public string Description { get; }

    /// <summary>
    ///     A flag to notify the installer that this step performs an app pool restart, this can be handy to know since if the
    ///     current
    ///     step is performing a restart, we cannot 'look ahead' to see if the next step can execute since we won't know until
    ///     the app pool
    ///     is restarted.
    /// </summary>
    public bool PerformsAppRestart { get; set; }
}
