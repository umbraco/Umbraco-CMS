﻿namespace Umbraco.Web.Security
{
    public static class AuthenticationOptionsExtensions
    {
        // TODO: Migrate this! This will basically be an implementation of sorts for IBackOfficeExternalLoginProviders

        //// these are used for backwards compat
        //private const string ExternalSignInAutoLinkOptionsProperty = "ExternalSignInAutoLinkOptions";
        //private const string ChallengeResultCallbackProperty = "ChallengeResultCallback";

        ///// <summary>
        ///// Used to specify all back office external login options
        ///// </summary>
        ///// <param name="authOptions"></param>
        ///// <param name="externalLoginProviderOptions"></param>
        //public static void SetBackOfficeExternalLoginProviderOptions(this AuthenticationOptions authOptions,
        //    BackOfficeExternalLoginProviderOptions externalLoginProviderOptions)
        //{
        //    authOptions.Description.Properties[Constants.Security.BackOfficeExternalLoginOptionsProperty] = externalLoginProviderOptions;

        //    // for backwards compat, we need to add these:
        //    // TODO: NOOOOOOOOOOO - that is not what we want to do in netcore when we figure out how to migrate this,
        //    // we'll also need to update angular to reference the correct properties, but that will all be figured out when we start testing
        //    if (externalLoginProviderOptions.AutoLinkOptions != null)
        //        authOptions.Description.Properties[ExternalSignInAutoLinkOptionsProperty] = externalLoginProviderOptions.AutoLinkOptions;
        //    if (externalLoginProviderOptions.OnChallenge != null)
        //        authOptions.Description.Properties[ChallengeResultCallbackProperty] = externalLoginProviderOptions.OnChallenge;
        //}

        //public static AuthenticationProperties GetSignInChallengeResult(this AuthenticationDescription authenticationDescription, IOwinContext ctx)
        //{
        //    if (authenticationDescription.Properties.ContainsKey(ChallengeResultCallbackProperty) == false) return null;
        //    var cb = authenticationDescription.Properties[ChallengeResultCallbackProperty] as Func<IOwinContext, AuthenticationProperties>;
        //    if (cb == null) return null;
        //    return cb(ctx);
        //}

        //public static ExternalSignInAutoLinkOptions GetExternalSignInAutoLinkOptions(this AuthenticationDescription authenticationDescription)
        //{
        //    if (authenticationDescription.Properties.ContainsKey(ExternalSignInAutoLinkOptionsProperty) == false) return null;
        //    var options = authenticationDescription.Properties[ExternalSignInAutoLinkOptionsProperty] as ExternalSignInAutoLinkOptions;
        //    return options;
        //}

        ///// <summary>
        ///// Configures the properties of the authentication description instance for use with Umbraco back office
        ///// </summary>
        ///// <param name="options"></param>
        ///// <param name="style"></param>
        ///// <param name="icon"></param>
        ///// <param name="callbackPath">
        ///// This is important if the identity provider is to be able to authenticate when upgrading Umbraco. We will try to extract this from
        ///// any options passed in via reflection since none of the default OWIN providers inherit from a base class but so far all of them have a consistent
        ///// name for the 'CallbackPath' property which is of type PathString. So we'll try to extract it if it's not found or supplied.
        /////
        ///// If a value is extracted or supplied, this will be added to an internal list which the UmbracoModule will use to allow the request to pass
        ///// through without redirecting to the installer.
        ///// </param>
        //public static void ForUmbracoBackOffice(this AuthenticationOptions options, string style, string icon, string callbackPath = null)
        //{
        //    if (options == null) throw new ArgumentNullException(nameof(options));
        //    if (string.IsNullOrEmpty(options.AuthenticationType)) throw new InvalidOperationException("The authentication type can't be null or empty.");

        //    //Ensure the prefix is set
        //    if (options.AuthenticationType.StartsWith(Constants.Security.BackOfficeExternalAuthenticationTypePrefix) == false)
        //    {
        //        options.AuthenticationType = Constants.Security.BackOfficeExternalAuthenticationTypePrefix + options.AuthenticationType;
        //    }

        //    options.Description.Properties["SocialStyle"] = style;
        //    options.Description.Properties["SocialIcon"] = icon;

        //    //flag for use in back office
        //    options.Description.Properties[Constants.Security.BackOfficeAuthenticationType] = true;

        //    if (callbackPath.IsNullOrWhiteSpace())
        //    {
        //        try
        //        {
        //            //try to get it with reflection
        //            var prop = options.GetType().GetProperty("CallbackPath");
        //            if (prop != null && TypeHelper.IsTypeAssignableFrom<PathString>(prop.PropertyType))
        //            {
        //                //get the value
        //                var path = (PathString) prop.GetValue(options);
        //                if (path.HasValue)
        //                {
        //                    RoutableDocumentFilter.ReservedPaths.TryAdd(path.ToString());
        //                }
        //            }
        //        }
        //        catch (System.Exception ex)
        //        {
        //            Current.Logger.LogError(ex, "Could not read AuthenticationOptions properties");
        //        }
        //    }
        //    else
        //    {
        //        RoutableDocumentFilter.ReservedPaths.TryAdd(callbackPath);
        //    }
        //}
    }
}
