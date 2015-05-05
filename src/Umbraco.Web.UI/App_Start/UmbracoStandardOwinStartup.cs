//using System.Security.Claims;
//using Microsoft.AspNet.Identity.Owin;
//using Microsoft.Owin;
//using Owin;
//using Umbraco.Core;
//using Umbraco.Core.Models.Identity;
//using Umbraco.Core.Security;
//using Umbraco.Web.UI;

////To use this startup class, change the appSetting value in the web.config called 
//// "owin:appStartup" to be "UmbracoStandardOwinStartup"

//[assembly: OwinStartup("UmbracoStandardOwinStartup", typeof(UmbracoStandardOwinStartup))]

//namespace Umbraco.Web.UI
//{
//    /// <summary>
//    /// The standard way to configure OWIN for Umbraco
//    /// </summary>
//    /// <remarks>
//    /// The startup type is specified in appSettings under owin:appStartup - change it to "StandardUmbracoStartup" to use this class
//    /// </remarks>
//    public class UmbracoStandardOwinStartup : UmbracoDefaultOwinStartup
//    {
//        public override void Configuration(IAppBuilder app)
//        {
//            //ensure the default options are configured
//            base.Configuration(app);

//            ////configure token auth
//            //app.ConfigureBackOfficeTokenAuth();
//        }
//    }
//}
