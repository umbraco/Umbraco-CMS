//using System;
//using System.Threading.Tasks;
//using Microsoft.Owin;
//using Microsoft.Owin.Security.OAuth;
//using Owin;
//using Umbraco.Web.Security.Identity;

//namespace Umbraco.Web.UI
//{
//    /// <summary>
//    /// Extension methods to configure Umbraco for issuing and processing tokens for authentication
//    /// </summary>    
//    public static class UmbracoAuthTokenServerExtensions
//    {

//        public static void ConfigureBackOfficeTokenAuth(this IAppBuilder app)
//        {
//            var oAuthServerOptions = new OAuthAuthorizationServerOptions()
//            {
//                //generally you wouldn't allow this unless on SSL!
//#if DEBUG
//                AllowInsecureHttp = true,
//#endif
//                TokenEndpointPath = new PathString("/umbraco/oauth/token"),
//                //set as different auth type to not interfere with anyone doing this on the front-end
//                AuthenticationType = Core.Constants.Security.BackOfficeTokenAuthenticationType,
//                AccessTokenExpireTimeSpan = TimeSpan.FromDays(1),
//                Provider = new BackOfficeAuthServerProvider()
//                {
//                    OnValidateClientAuthentication = context =>
//                    {
//                        // Called to validate that the origin of the request is a registered "client_id", and that the correct credentials for that client are
//                        // present on the request. If the web application accepts Basic authentication credentials, 
//                        // context.TryGetBasicCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request header. If the web 
//                        // application accepts "client_id" and "client_secret" as form encoded POST parameters, 
//                        // context.TryGetFormCredentials(out clientId, out clientSecret) may be called to acquire those values if present in the request body.
//                        // If context.Validated is not called the request will not proceed further. 

//                        //** Currently we just accept everything globally
//                        context.Validated();
//                        return Task.FromResult(0);

//                        // Example for checking registered clients:

//                        //** Validate that the data is in the request
//                        //string clientId;
//                        //string clientSecret;
//                        //if (context.TryGetFormCredentials(out clientId, out clientSecret) == false)
//                        //{
//                        //    context.SetError("invalid_client", "Form credentials could not be retrieved.");
//                        //    context.Rejected();
//                        //    return Task.FromResult(0);
//                        //}

//                        //var userManager = context.OwinContext.GetUserManager<BackOfficeUserManager>();

//                        //** Check if this client id is allowed/registered
//                        // - lookup in custom table

//                        //** Verify that the client id and client secret match 
//                        //if (client != null && userManager.PasswordHasher.VerifyHashedPassword(client.ClientSecretHash, clientSecret) == PasswordVerificationResult.Success)
//                        //{
//                        //    // Client has been verified.
//                        //    context.Validated(clientId);
//                        //}
//                        //else
//                        //{
//                        //    // Client could not be validated.
//                        //    context.SetError("invalid_client", "Client credentials are invalid.");
//                        //    context.Rejected();
//                        //}
//                    }
//                }
//            };

//            // Token Generation
//            app.UseOAuthAuthorizationServer(oAuthServerOptions);
//            app.UseOAuthBearerAuthentication(new OAuthBearerAuthenticationOptions());

//        }
//    }
//}