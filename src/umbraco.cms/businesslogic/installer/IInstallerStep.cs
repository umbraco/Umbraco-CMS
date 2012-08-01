using System;
namespace umbraco.cms.businesslogic.installer
{
    interface IInstallerStep
    {
        string Alias { get;}
        bool Completed();
        string Name { get;}
        string UserControl {get;}

        bool HideFromNavigation { get; }
    }
}
