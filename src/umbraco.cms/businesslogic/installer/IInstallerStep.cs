using System;
namespace umbraco.cms.businesslogic.installer
{
    [Obsolete("This is no longer used and will be removed from the codebase in future versions.")]
    interface IInstallerStep
    {
        string Alias { get;}
        bool Completed();
        string Name { get;}
        string UserControl {get;}

        bool HideFromNavigation { get; }
    }
}
