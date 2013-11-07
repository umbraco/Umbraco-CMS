using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A base membership provider class offering much of the underlying functionality for initializing and password encryption/hashing.
    /// </summary>
    public abstract class MembershipProviderBase : MembershipProvider
    {
        /// <summary>
        /// Providers can override this setting, default is 7
        /// </summary>
        protected virtual int DefaultMinPasswordLength
        {
            get { return 7; }
        }

        /// <summary>
        /// Providers can override this setting, default is 1
        /// </summary>
        protected virtual int DefaultMinNonAlphanumericChars
        {
            get { return 1; }
        }

        /// <summary>
        /// Providers can override this setting, default is false to use better security
        /// </summary>
        protected virtual bool DefaultUseLegacyEncoding
        {
            get { return false; }
        }

        /// <summary>
        /// Providers can override this setting, by default this is false which means that the provider will 
        /// authenticate the username + password when ChangePassword is called. This property exists purely for
        /// backwards compatibility.
        /// </summary>
        internal virtual bool AllowManuallyChangingPassword
        {
            get { return false; }
        }

        private string _applicationName;
        private bool _enablePasswordReset;
        private bool _enablePasswordRetrieval;
        private int _maxInvalidPasswordAttempts;
        private int _minRequiredNonAlphanumericCharacters;
        private int _minRequiredPasswordLength;
        private int _passwordAttemptWindow;
        private MembershipPasswordFormat _passwordFormat;
        private string _passwordStrengthRegularExpression;
        private bool _requiresQuestionAndAnswer;
        private bool _requiresUniqueEmail;
        private bool _useLegacyEncoding;

        #region Properties


        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to reset their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider supports password reset; otherwise, false. The default is true.</returns>
        public override bool EnablePasswordReset
        {
            get { return _enablePasswordReset; }
        }

        /// <summary>
        /// Indicates whether the membership provider is configured to allow users to retrieve their passwords.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider is configured to support password retrieval; otherwise, false. The default is false.</returns>
        public override bool EnablePasswordRetrieval
        {
            get { return _enablePasswordRetrieval; }
        }

        /// <summary>
        /// Gets the number of invalid password or password-answer attempts allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of invalid password or password-answer attempts allowed before the membership user is locked out.</returns>
        public override int MaxInvalidPasswordAttempts
        {
            get { return _maxInvalidPasswordAttempts; }
        }

        /// <summary>
        /// Gets the minimum number of special characters that must be present in a valid password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum number of special characters that must be present in a valid password.</returns>
        public override int MinRequiredNonAlphanumericCharacters
        {
            get { return _minRequiredNonAlphanumericCharacters; }
        }

        /// <summary>
        /// Gets the minimum length required for a password.
        /// </summary>
        /// <value></value>
        /// <returns>The minimum length required for a password. </returns>
        public override int MinRequiredPasswordLength
        {
            get { return _minRequiredPasswordLength; }
        }

        /// <summary>
        /// Gets the number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.
        /// </summary>
        /// <value></value>
        /// <returns>The number of minutes in which a maximum number of invalid password or password-answer attempts are allowed before the membership user is locked out.</returns>
        public override int PasswordAttemptWindow
        {
            get { return _passwordAttemptWindow; }
        }

        /// <summary>
        /// Gets a value indicating the format for storing passwords in the membership data store.
        /// </summary>
        /// <value></value>
        /// <returns>One of the <see cref="T:System.Web.Security.MembershipPasswordFormat"></see> values indicating the format for storing passwords in the data store.</returns>
        public override MembershipPasswordFormat PasswordFormat
        {
            get { return _passwordFormat; }
        }

        /// <summary>
        /// Gets the regular expression used to evaluate a password.
        /// </summary>
        /// <value></value>
        /// <returns>A regular expression used to evaluate a password.</returns>
        public override string PasswordStrengthRegularExpression
        {
            get { return _passwordStrengthRegularExpression; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require the user to answer a password question for password reset and retrieval.
        /// </summary>
        /// <value></value>
        /// <returns>true if a password answer is required for password reset and retrieval; otherwise, false. The default is true.</returns>
        public override bool RequiresQuestionAndAnswer
        {
            get { return _requiresQuestionAndAnswer; }
        }

        /// <summary>
        /// Gets a value indicating whether the membership provider is configured to require a unique e-mail address for each user name.
        /// </summary>
        /// <value></value>
        /// <returns>true if the membership provider requires a unique e-mail address; otherwise, false. The default is true.</returns>
        public override bool RequiresUniqueEmail
        {
            get { return _requiresUniqueEmail; }
        }

        /// <summary>
        /// The name of the application using the custom membership provider.
        /// </summary>
        /// <value></value>
        /// <returns>The name of the application using the custom membership provider.</returns>
        public override string ApplicationName
        {
            get
            {
                return _applicationName;
            }
            set
            {
                if (string.IsNullOrEmpty(value))
                    throw new ProviderException("ApplicationName cannot be empty.");

                if (value.Length > 0x100)
                    throw new ProviderException("Provider application name too long.");

                _applicationName = value;
            }
        }

        #endregion

        /// <summary>
        /// Initializes the provider.
        /// </summary>
        /// <param name="name">The friendly name of the provider.</param>
        /// <param name="config">A collection of the name/value pairs representing the provider-specific attributes specified in the configuration for this provider.</param>
        /// <exception cref="T:System.ArgumentNullException">The name of the provider is null.</exception>
        /// <exception cref="T:System.InvalidOperationException">An attempt is made to call 
        /// <see cref="M:System.Configuration.Provider.ProviderBase.Initialize(System.String,System.Collections.Specialized.NameValueCollection)"></see> on a provider after the provider 
        /// has already been initialized.</exception>
        /// <exception cref="T:System.ArgumentException">The name of the provider has a length of zero.</exception>
        public override void Initialize(string name, NameValueCollection config)
        {            
            // Initialize base provider class
            base.Initialize(name, config);

            _enablePasswordRetrieval = config.GetValue("enablePasswordRetrieval", false);
            _enablePasswordReset = config.GetValue("enablePasswordReset", false);
            _requiresQuestionAndAnswer = config.GetValue("requiresQuestionAndAnswer", false);
            _requiresUniqueEmail = config.GetValue("requiresUniqueEmail", false);
            _maxInvalidPasswordAttempts = GetIntValue(config, "maxInvalidPasswordAttempts", 5, false, 0);
            _passwordAttemptWindow = GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _minRequiredPasswordLength = GetIntValue(config, "minRequiredPasswordLength", DefaultMinPasswordLength, true, 0x80);
            _minRequiredNonAlphanumericCharacters = GetIntValue(config, "minRequiredNonalphanumericCharacters", DefaultMinNonAlphanumericChars, true, 0x80);
            _passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];

            _applicationName = config["applicationName"];
            if (string.IsNullOrEmpty(_applicationName))
                _applicationName = GetDefaultAppName();

            //by default we will continue using the legacy encoding.
            _useLegacyEncoding = config.GetValue("useLegacyEncoding", DefaultUseLegacyEncoding);

            // make sure password format is clear by default.
            string str = config["passwordFormat"] ?? "Clear";

            switch (str.ToLower())
            {
                case "clear":
                    _passwordFormat = MembershipPasswordFormat.Clear;
                    break;

                case "encrypted":
                    _passwordFormat = MembershipPasswordFormat.Encrypted;
                    break;

                case "hashed":
                    _passwordFormat = MembershipPasswordFormat.Hashed;
                    break;

                default:
                    throw new ProviderException("Provider bad password format");
            }

            if ((PasswordFormat == MembershipPasswordFormat.Hashed) && EnablePasswordRetrieval)
                throw new ProviderException("Provider can not retrieve hashed password");

        }

        /// <summary>
        /// Override this method to ensure the password is valid before raising the event
        /// </summary>
        /// <param name="e"></param>
        protected override void OnValidatingPassword(ValidatePasswordEventArgs e)
        {
            var attempt = IsPasswordValid(e.Password, MinRequiredNonAlphanumericCharacters, PasswordStrengthRegularExpression, MinRequiredPasswordLength);
            if (attempt.Success == false)
            {
                e.Cancel = true;
                return;
            }

            base.OnValidatingPassword(e);
        }

        protected internal enum PasswordValidityError
        {
            Ok,
            Length,
            AlphanumericChars,
            Strength
        }

        /// <summary>
        /// Checks to ensure the AllowManuallyChangingPassword rule is adhered to
        /// </summary>
        /// <param name="username"></param>
        /// <param name="oldPassword"></param>
        /// <param name="newPassword"></param>
        /// <returns></returns>
        public sealed override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (oldPassword.IsNullOrWhiteSpace() && AllowManuallyChangingPassword == false)
            {
                //If the old password is empty and AllowManuallyChangingPassword is false, than this provider cannot just arbitrarily change the password
                throw new NotSupportedException("This provider does not support manually changing the password");
            }

            return PerformChangePassword(username, oldPassword, newPassword);
        }

        protected abstract bool PerformChangePassword(string username, string oldPassword, string newPassword);

        protected internal static Attempt<PasswordValidityError> IsPasswordValid(string password, int minRequiredNonAlphanumericChars, string strengthRegex, int minLength)
        {
            if (minRequiredNonAlphanumericChars > 0)
            {
                var nonAlphaNumeric = Regex.Replace(password, "[a-zA-Z0-9]", "", RegexOptions.Multiline | RegexOptions.IgnoreCase);
                if (nonAlphaNumeric.Length < minRequiredNonAlphanumericChars)
                {
                    return Attempt.Fail(PasswordValidityError.AlphanumericChars);
                }
            }

            if (string.IsNullOrEmpty(strengthRegex) == false)
            {
                if (Regex.IsMatch(password, strengthRegex, RegexOptions.Compiled) == false)
                {
                    return Attempt.Fail(PasswordValidityError.Strength);
                }
                
            }

            if (password.Length < minLength)
            {
                return Attempt.Fail(PasswordValidityError.Length);
            }

            return Attempt.Succeed(PasswordValidityError.Ok);
        }

        /// <summary>
        /// Gets the name of the default app.
        /// </summary>
        /// <returns></returns>
        internal static string GetDefaultAppName()
        {
            try
            {
                string applicationVirtualPath = HostingEnvironment.ApplicationVirtualPath;
                if (string.IsNullOrEmpty(applicationVirtualPath))
                {
                    return "/";
                }
                return applicationVirtualPath;
            }
            catch
            {
                return "/";
            }
        }

        internal static int GetIntValue(NameValueCollection config, string valueName, int defaultValue, bool zeroAllowed, int maxValueAllowed)
        {
            int num;
            string s = config[valueName];
            if (s == null)
            {
                return defaultValue;
            }
            if (!int.TryParse(s, out num))
            {
                if (zeroAllowed)
                {
                    throw new ProviderException("Value must be non negative integer");
                }
                throw new ProviderException("Value must be positive integer");
            }
            if (zeroAllowed && (num < 0))
            {
                throw new ProviderException("Value must be non negativeinteger");
            }
            if (!zeroAllowed && (num <= 0))
            {
                throw new ProviderException("Value must be positive integer");
            }
            if ((maxValueAllowed > 0) && (num > maxValueAllowed))
            {
                throw new ProviderException("Value too big");
            }
            return num;
        }

        protected string FormatPasswordForStorage(string pass, string salt)
        {
            if (_useLegacyEncoding)
            {
                return pass;
            }

            //the better way, we use salt per member
            return salt + pass;
        }

        protected string EncryptOrHashPassword(string pass, string salt)
        {
            //if we are doing it the old way

            if (_useLegacyEncoding)
            {
                return LegacyEncodePassword(pass);
            }

            //This is the correct way to implement this (as per the sql membership provider)

            if ((int)PasswordFormat == 0)
                return pass;
            var bytes = Encoding.Unicode.GetBytes(pass);
            var numArray1 = Convert.FromBase64String(salt);
            byte[] inArray;
            if ((int)PasswordFormat == 1)
            {
                var hashAlgorithm = GetHashAlgorithm(pass);
                var algorithm = hashAlgorithm as KeyedHashAlgorithm;
                if (algorithm != null)
                {
                    var keyedHashAlgorithm = algorithm;
                    if (keyedHashAlgorithm.Key.Length == numArray1.Length)
                        keyedHashAlgorithm.Key = numArray1;
                    else if (keyedHashAlgorithm.Key.Length < numArray1.Length)
                    {
                        var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                        Buffer.BlockCopy(numArray1, 0, numArray2, 0, numArray2.Length);
                        keyedHashAlgorithm.Key = numArray2;
                    }
                    else
                    {
                        var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                        var dstOffset = 0;
                        while (dstOffset < numArray2.Length)
                        {
                            var count = Math.Min(numArray1.Length, numArray2.Length - dstOffset);
                            Buffer.BlockCopy(numArray1, 0, numArray2, dstOffset, count);
                            dstOffset += count;
                        }
                        keyedHashAlgorithm.Key = numArray2;
                    }
                    inArray = keyedHashAlgorithm.ComputeHash(bytes);
                }
                else
                {
                    var buffer = new byte[numArray1.Length + bytes.Length];
                    Buffer.BlockCopy(numArray1, 0, buffer, 0, numArray1.Length);
                    Buffer.BlockCopy(bytes, 0, buffer, numArray1.Length, bytes.Length);
                    inArray = hashAlgorithm.ComputeHash(buffer);
                }
            }
            else
            {
                var password = new byte[numArray1.Length + bytes.Length];
                Buffer.BlockCopy(numArray1, 0, password, 0, numArray1.Length);
                Buffer.BlockCopy(bytes, 0, password, numArray1.Length, bytes.Length);
                inArray = EncryptPassword(password, MembershipPasswordCompatibilityMode.Framework40);
            }
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Encrypt/hash a new password with a new salt
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        protected string EncryptOrHashNewPassword(string newPassword, out string salt)
        {
            salt = GenerateSalt();
            return EncryptOrHashPassword(newPassword, salt);
        }

        /// <summary>
        /// Gets the encrypted or hashed string of an existing password for an existing user
        /// </summary>
        /// <param name="storedPassword">The stored string for the password</param>
        /// <returns></returns>
        protected string EncryptOrHashExistingPassword(string storedPassword)
        {
            if (_useLegacyEncoding)
            {
                return EncryptOrHashPassword(storedPassword, storedPassword);
            }
            else
            {
                string salt;
                var pass = StoredPassword(storedPassword, PasswordFormat, out salt);
                return EncryptOrHashPassword(pass, salt);    
            }
        }

        protected string DecodePassword(string pass)
        {
            //if we are doing it the old way

            if (_useLegacyEncoding)
            {
                return LegacyUnEncodePassword(pass);
            }

            //This is the correct way to implement this (as per the sql membership provider)

            switch ((int)PasswordFormat)
            {
                case 0:
                    return pass;
                case 1:
                    throw new ProviderException("Provider can not decode hashed password");
                default:
                    var bytes = DecryptPassword(Convert.FromBase64String(pass));
                    return bytes == null ? null : Encoding.Unicode.GetString(bytes, 16, bytes.Length - 16);
            }
        }

        /// <summary>
        /// Returns the hashed password without the salt if it is hashed
        /// </summary>
        /// <param name="storedString"></param>
        /// <param name="format"></param>
        /// <param name="salt">returns the salt</param>
        /// <returns></returns>
        internal static string StoredPassword(string storedString, MembershipPasswordFormat format, out string salt)
        {
            switch (format)
            {
                case MembershipPasswordFormat.Hashed:
                    var saltLen = GenerateSalt();
                    salt = storedString.Substring(0, saltLen.Length);
                    return storedString.Substring(saltLen.Length);  
                case MembershipPasswordFormat.Clear:                    
                case MembershipPasswordFormat.Encrypted:
                default:
                   salt = string.Empty;
                    return storedString;
                     
            }
        }

        protected internal static string GenerateSalt()
        {
            var numArray = new byte[16];
            new RNGCryptoServiceProvider().GetBytes(numArray);
            return Convert.ToBase64String(numArray);
        }

        protected HashAlgorithm GetHashAlgorithm(string password)
        {
            if (_useLegacyEncoding)
            {
                //before we were never checking for an algorithm type so we were always using HMACSHA1
                // for any SHA specified algorithm :( so we'll need to keep doing that for backwards compat support.
                if (Membership.HashAlgorithmType.InvariantContains("SHA"))
                {
                    return new HMACSHA1
                        {
                            //the legacy salt was actually the password :(
                            Key = Encoding.Unicode.GetBytes(password)
                        };
                }               
            }

            //get the algorithm by name

            return HashAlgorithm.Create(Membership.HashAlgorithmType);
        }

        /// <summary>
        /// Encodes the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <returns>The encoded password.</returns>
        protected string LegacyEncodePassword(string password)
        {
            string encodedPassword = password;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    encodedPassword =
                        Convert.ToBase64String(EncryptPassword(Encoding.Unicode.GetBytes(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    var hashAlgorith = GetHashAlgorithm(password);
                    encodedPassword = Convert.ToBase64String(hashAlgorith.ComputeHash(Encoding.Unicode.GetBytes(password)));
                    break;
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return encodedPassword;
        }

        /// <summary>
        /// Unencode password.
        /// </summary>
        /// <param name="encodedPassword">The encoded password.</param>
        /// <returns>The unencoded password.</returns>
        protected string LegacyUnEncodePassword(string encodedPassword)
        {
            string password = encodedPassword;
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Clear:
                    break;
                case MembershipPasswordFormat.Encrypted:
                    password = Encoding.Unicode.GetString(DecryptPassword(Convert.FromBase64String(password)));
                    break;
                case MembershipPasswordFormat.Hashed:
                    throw new ProviderException("Cannot unencode a hashed password.");
                default:
                    throw new ProviderException("Unsupported password format.");
            }
            return password;
        }

    }
}