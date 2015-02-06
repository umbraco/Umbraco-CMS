using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Microsoft.Owin.Security.Cookies;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;
using Umbraco.Core.Logging;

namespace Umbraco.Web.Security.Identity
{
    public static class AppBuilderExtensions
    {
        ///// <summary>
        ///// Configure Identity User Manager for Umbraco
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="app"></param>
        ///// <param name="appContext"></param>
        //public static void ConfigureUserManagerForUmbraco<T>(this IAppBuilder app, ApplicationContext appContext)
        //    where T : UmbracoIdentityUser, new()
        //{

        //    //Don't proceed if the app is not ready
        //    if (appContext.IsConfigured == false
        //        || appContext.DatabaseContext == null
        //        || appContext.DatabaseContext.IsDatabaseConfigured == false) return;

        //    //Configure Umbraco user manager to be created per request
        //    app.CreatePerOwinContext<UmbracoMembersUserManager<T>>(
        //        (o, c) => UmbracoMembersUserManager<T>.Create(
        //            o, c, ApplicationContext.Current.Services.MemberService));

        //    //Configure Umbraco member event handler to be created per request - this will ensure that the
        //    // external logins are kept in sync if members are deleted from Umbraco
        //    app.CreatePerOwinContext<MembersEventHandler<T>>((options, context) => new MembersEventHandler<T>(context));

        //    //TODO: This is just for the mem leak fix
        //    app.CreatePerOwinContext<OwinContextDisposal<MembersEventHandler<T>, UmbracoMembersUserManager<T>>>(
        //        (o, c) => new OwinContextDisposal<MembersEventHandler<T>, UmbracoMembersUserManager<T>>(c));
        //}

        /// <summary>
        /// Ensures that the UmbracoBackOfficeAuthenticationMiddleware is assigned to the pipeline
        /// </summary>
        /// <param name="app"></param>
        /// <returns></returns>
        public static IAppBuilder UseUmbracoBackAuthentication(this IAppBuilder app)
        {
            if (app == null) throw new ArgumentNullException("app");


            app.UseCookieAuthentication(new UmbracoBackOfficeCookieAuthenticationOptions(
                    UmbracoConfig.For.UmbracoSettings().Security,
                    GlobalSettings.TimeOutInMinutes,
                    GlobalSettings.UseSSL,
                    GlobalSettings.Path)
            {
                //Provider = new CookieAuthenticationProvider
                //{
                //    // Enables the application to validate the security stamp when the user 
                //    // logs in. This is a security feature which is used when you 
                //    // change a password or add an external login to your account.  
                //    OnValidateIdentity = SecurityStampValidator
                //        .OnValidateIdentity<UmbracoMembersUserManager<UmbracoApplicationUser>, UmbracoApplicationUser, int>(
                //            TimeSpan.FromMinutes(30),
                //            (manager, user) => user.GenerateUserIdentityAsync(manager),
                //            identity => identity.GetUserId<int>())
                //}
            });

            return app;
        }

    }
}