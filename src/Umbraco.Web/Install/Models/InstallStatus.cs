using System;
using System.Collections;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel.ComIntegration;
using System.Text;
using System.Threading.Tasks;

namespace Umbraco.Web.Install.Models
{
    public enum InstallStatus
    {
        NewInstall,
        Upgrade,

        /// <summary>
        /// Used when we detect an upgrade but there is no user token associated
        /// </summary>
        UpgradeWithoutToken,

        ///// <summary>
        ///// Used if the user presses f5 and is in the middle of an install
        ///// </summary>
        //InProgress
    }
}
