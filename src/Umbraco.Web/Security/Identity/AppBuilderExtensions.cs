using System;
using System.Collections.Generic;
using System.Web;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Microsoft.Owin.Extensions;
using Owin;
using Umbraco.Core;
using Umbraco.Core.Configuration;

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

            app.Use(typeof (UmbracoBackOfficeAuthenticationMiddleware),
                //ctor params
                app, new UmbracoBackOfficeAuthenticationOptions(), UmbracoConfig.For.UmbracoSettings().Security);

            app.UseStageMarker(PipelineStage.Authenticate);
            return app;
        }

        //This is a fix for OWIN mem leak! 
        //http://stackoverflow.com/questions/24378856/memory-leak-in-owin-appbuilderextensions/24819543#24819543
        private class OwinContextDisposal<T1, T2> : IDisposable
            where T1 : IDisposable
            where T2 : IDisposable
        {
            private readonly List<IDisposable> _disposables = new List<IDisposable>();
            private bool _disposed = false;

            public OwinContextDisposal(IOwinContext owinContext)
            {
                if (HttpContext.Current == null) return;

                _disposables.Add(owinContext.Get<T1>());
                _disposables.Add(owinContext.Get<T2>());

                HttpContext.Current.DisposeOnPipelineCompleted(this);
            }

            public void Dispose()
            {
                if (_disposed) return;
                foreach (var disposable in _disposables)
                {
                    disposable.Dispose();
                }
                _disposed = true;
            }
        }
    }
}