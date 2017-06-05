using System;
using System.Collections.Generic;
using System.Linq;
using System.Web.Http;
using Google.Authenticator;
using Umbraco.Core.Logging;
using Umbraco.Core.Models.Rdbms;
using Umbraco.Web.Models;
using Umbraco.Web.Mvc;
using Umbraco.Web.WebApi;
using Umbraco.Web.WebApi.Filters;

namespace Umbraco.Web.Editors
{
    [PluginController("UmbracoApi")]
    [ValidationFilter]
    [AngularJsonOnlyConfiguration]
    [IsBackOffice]
    public class TwoFactorAuthenticationController : UmbracoAuthorizedApiController
    {
        private const string GoogleAuthProviderName = "GoogleAuthenticator";

        [HttpGet]
        public List<TwoFactorAuthenticationInfo> TwoFactorEnabled()
        {
            var database = DatabaseContext.Database;
            var user = Security.CurrentUser;
            var result = database.Fetch<TwoFactorDto>("WHERE [userId] = @userId AND [confirmed] = 1", new { userId = user.Id });
            var twoFactorAuthInfo = new List<TwoFactorAuthenticationInfo>();
            foreach (var factor in result)
            {
                var authInfo = new TwoFactorAuthenticationInfo { ApplicationName = factor.Key };
                twoFactorAuthInfo.Add(authInfo);
            }
            return twoFactorAuthInfo;
        }


        [HttpGet]
        public TwoFactorAuthenticationInfo GoogleAuthenticatorSetupCode()
        {
            var tfa = new TwoFactorAuthenticator();
            var user = Security.CurrentUser;
            var accountSecretKey = Guid.NewGuid().ToString();
            var applicationName = "UmbracoGoogleAuthenticator";
            var setupInfo = tfa.GenerateSetupCode(applicationName, user.Email, accountSecretKey, 300, 300);

            var database = DatabaseContext.Database;
            var twoFactorAuthInfo = new TwoFactorAuthenticationInfo();

            var existingAccount = database.Fetch<TwoFactorDto>(string.Format("WHERE userId = {0} AND [key] = '{1}'",
                user.Id, GoogleAuthProviderName));
            if (existingAccount.Any())
            {
                var account = existingAccount.First();
                if (account.Confirmed)
                    return twoFactorAuthInfo;

                var tf = new TwoFactorDto {Value = accountSecretKey, UserId = user.Id, Key = GoogleAuthProviderName};
                var update = database.Update(tf);

                if (update == 0)
                    return twoFactorAuthInfo;
            }
            else
            {
                var result = database.Insert(new TwoFactorDto { UserId = user.Id, Key = GoogleAuthProviderName, Value = accountSecretKey, Confirmed = false });
                if (result is bool == false)
                    return twoFactorAuthInfo;

                var insertSucces = (bool)result;
                if (insertSucces == false)
                    return twoFactorAuthInfo;

            }

            twoFactorAuthInfo.Secret = setupInfo.ManualEntryKey;
            twoFactorAuthInfo.Email = user.Email;
            twoFactorAuthInfo.ApplicationName = applicationName;

            return twoFactorAuthInfo;
        }

        [HttpPost]
        public bool ValidateAndSaveGoogleAuth(string code)
        {
            var database = DatabaseContext.Database;
            var user = Security.CurrentUser;
            try
            {
                var twoFactorAuthenticator = new TwoFactorAuthenticator();
                var all = database.Fetch<TwoFactorDto>("WHERE userId = @userId",
                    new { userId = user.Id });

                var result = all.LastOrDefault(t => t.Key == GoogleAuthProviderName);
                if (result != null)
                {
                    var isValid = twoFactorAuthenticator.ValidateTwoFactorPIN(result.Value, code);
                    if (isValid == false)
                        return false;

                    var tf = new TwoFactorDto { Confirmed = true, Value = result.Value, UserId = user.Id, Key = GoogleAuthProviderName };
                    var update = database.Update(tf);
                    isValid = update > 0;
                    return isValid;
                }
            }
            catch (Exception ex)
            {
                LogHelper.Error<TwoFactorAuthenticationController>("Could not log in with the provided one-time-password", ex);
            }
            return false;
        }

        [HttpPost]
        public bool Disable()
        {
            var database = DatabaseContext.Database;
            var user = Security.CurrentUser;
            var result = database.Delete<TwoFactorDto>("WHERE [userId] = @userId", new { userId = user.Id });
            //if more than 0 rows have been deleted, the query ran successfully
            return result != 0;
        }
    }
}
