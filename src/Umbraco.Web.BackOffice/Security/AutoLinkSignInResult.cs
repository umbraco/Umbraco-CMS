using Microsoft.AspNetCore.Identity;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Web.Common.Security
{
    public class AutoLinkSignInResult : SignInResult
    {
        public static AutoLinkSignInResult FailedNotLinked = new AutoLinkSignInResult()
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedNoEmail = new AutoLinkSignInResult()
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedException(string error) => new(new[] { error })
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedCreatingUser(IReadOnlyCollection<string> errors) => new(errors)
        {
            Succeeded = false
        };

        public static AutoLinkSignInResult FailedLinkingUser(IReadOnlyCollection<string> errors) => new(errors)
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
