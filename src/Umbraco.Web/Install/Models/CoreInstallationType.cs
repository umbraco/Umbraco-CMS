using System;

namespace Umbraco.Web.Install.Models
{
    [Flags]
    public enum CoreInstallationType
    {
        NewInstall =    1 << 0,    // 1
        Upgrade =       1 << 1,       // 2        
    }
}