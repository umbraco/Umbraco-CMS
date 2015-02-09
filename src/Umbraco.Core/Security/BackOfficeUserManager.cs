using System;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Web.Security;
using Microsoft.AspNet.Identity;
using Microsoft.AspNet.Identity.Owin;
using Microsoft.Owin;
using Umbraco.Core.Models.Identity;
using Umbraco.Core.Services;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A custom password hasher that conforms to the current password hashing done in Umbraco
    /// </summary>
    internal class MembershipPasswordHasher : IPasswordHasher
    {
        private readonly MembershipProviderBase _provider;

        public MembershipPasswordHasher(MembershipProviderBase provider)
        {
            _provider = provider;
        }

        public string HashPassword(string password)
        {
            return _provider.HashPasswordForStorage(password);
        }

        public PasswordVerificationResult VerifyHashedPassword(string hashedPassword, string providedPassword)
        {
            return _provider.VerifyPassword(providedPassword, hashedPassword)
                ? PasswordVerificationResult.Success
                : PasswordVerificationResult.Failed;
        }


    }

    /// <summary>
    /// Back office user manager
    /// </summary>
    public class BackOfficeUserManager : UserManager<BackOfficeIdentityUser, int>
    {
        public BackOfficeUserManager(IUserStore<BackOfficeIdentityUser, int> store)
            : base(store)
        {
        }

        #region What we support currently

        //TODO: Support this
        public override bool SupportsUserRole
        {
            get { return false; }
        }

        //TODO: Support this
        public override bool SupportsQueryableUsers
        {
            get { return false; }
        }

        //TODO: Support this
        public override bool SupportsUserLockout
        {
            get { return false; }
        }

        //TODO: Support this
        public override bool SupportsUserSecurityStamp
        {
            get { return false; }
        }

        public override bool SupportsUserTwoFactor
        {
            get { return false; }
        }

        public override bool SupportsUserPhoneNumber
        {
            get { return false; }
        } 
        #endregion

        public static BackOfficeUserManager Create(
            IdentityFactoryOptions<BackOfficeUserManager> options,
            IOwinContext context,
            IUserService userService,
            IExternalLoginService externalLoginService,
            MembershipProviderBase membershipProvider)
        {
            if (options == null) throw new ArgumentNullException("options");
            if (context == null) throw new ArgumentNullException("context");
            if (userService == null) throw new ArgumentNullException("userService");
            if (externalLoginService == null) throw new ArgumentNullException("externalLoginService");

            var manager = new BackOfficeUserManager(new BackOfficeUserStore(userService, externalLoginService, membershipProvider));

            // Configure validation logic for usernames
            manager.UserValidator = new UserValidator<BackOfficeIdentityUser, int>(manager)
            {
                AllowOnlyAlphanumericUserNames = false,
                RequireUniqueEmail = true
            };

            // Configure validation logic for passwords
            manager.PasswordValidator = new PasswordValidator
            {
                RequiredLength = membershipProvider.MinRequiredPasswordLength,
                RequireNonLetterOrDigit = membershipProvider.MinRequiredNonAlphanumericCharacters > 0,
                RequireDigit = false,
                RequireLowercase = false,
                RequireUppercase = false
            };

            //use a custom hasher based on our membership provider
            manager.PasswordHasher = new MembershipPasswordHasher(membershipProvider);

            var dataProtectionProvider = options.DataProtectionProvider;
            if (dataProtectionProvider != null)
            {
                manager.UserTokenProvider = new DataProtectorTokenProvider<BackOfficeIdentityUser, int>(dataProtectionProvider.Create("ASP.NET Identity"));
            }

            //custom identity factory for creating the identity object for which we auth against in the back office
            manager.ClaimsIdentityFactory = new BackOfficeClaimsIdentityFactory();

            //NOTE: Not implementing these currently

            //// Register two factor authentication providers. This application uses Phone and Emails as a step of receiving a code for verifying the user
            //// You can write your own provider and plug in here.
            //manager.RegisterTwoFactorProvider("PhoneCode", new PhoneNumberTokenProvider<ApplicationUser>
            //{
            //    MessageFormat = "Your security code is: {0}"
            //});
            //manager.RegisterTwoFactorProvider("EmailCode", new EmailTokenProvider<ApplicationUser>
            //{
            //    Subject = "Security Code",
            //    BodyFormat = "Your security code is: {0}"
            //});

            //manager.EmailService = new EmailService();
            //manager.SmsService = new SmsService();

            return manager;
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
        }
    }
}
