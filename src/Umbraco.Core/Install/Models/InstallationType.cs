namespace Umbraco.Cms.Core.Install.Models;

[Obsolete("This will no longer be used with the new backoffice APi, install steps and upgrade steps is instead two different interfaces.")]
[Flags]
public enum InstallationType
{
    NewInstall = 1 << 0, // 1
    Upgrade = 1 << 1, // 2
}
