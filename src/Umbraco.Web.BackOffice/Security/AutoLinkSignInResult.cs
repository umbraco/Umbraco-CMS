using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;

namespace Umbraco.Cms.Web.BackOffice.Security
{
    /// <summary>
    /// Result returned from signing in when auto-linking takes place
    /// </summary>
    public class AutoLinkSignInResult : SignInResult
    {
        public static AutoLinkSignInResult FailedNotLinked => new AutoLinkSignInResult()
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedNoEmail => new AutoLinkSignInResult()
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedException(string error) => new AutoLinkSignInResult(new[] { error })
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) => new AutoLinkSignInResult(errors)
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) => new AutoLinkSignInResult(errors)
        {
            Succeeded = false
        };

        public AutoLinkSignInResult(IReadOnlyCollection<string> errors)
        {
            Errors = errors ?? throw new ArgumentNullException(nameof(errors));
        }

        public AutoLinkSignInResult()
        {
        }

        public IReadOnlyCollection<string> Errors { get; } = Array.Empty<string>();
    }
}
