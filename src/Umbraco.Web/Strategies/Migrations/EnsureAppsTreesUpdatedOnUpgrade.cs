using Umbraco.Core.Persistence.Migrations.Upgrades.TargetVersionSix;
using umbraco.BusinessLogic;
using umbraco.interfaces;

namespace Umbraco.Web.Strategies.Migrations
{
    /// <summary>
    /// This is kind of a hack to ensure that Apps and Trees that might still reside in the db is
    /// written to the 'new' applications.config and trees.config files upon upgrade to version 6.0
    /// </summary>
    public class EnsureAppsTreesUpdatedOnUpgrade : IApplicationStartupHandler
    {
        public EnsureAppsTreesUpdatedOnUpgrade()
        {
            //This event will be triggered once the EnsureAppsTreesUpdated Up() method is run during upgrades
            EnsureAppsTreesUpdated.Upgrading += EnsureAppsTreesUpdated_Upgrading;
        }

        void EnsureAppsTreesUpdated_Upgrading(object sender, EnsureAppsTreesUpdated.UpgradingEventArgs e)
        {
            var treeRegistrar = new ApplicationTreeRegistrar();
            var appRegistrar = new ApplicationRegistrar();
        }
    }
}