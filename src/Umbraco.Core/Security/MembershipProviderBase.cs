using System;
using System.Collections.Specialized;
using System.Configuration.Provider;
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;
using System.Web.Configuration;
using System.Web.Hosting;
using System.Web.Security;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Security
{
    /// <summary>
    /// A base membership provider class offering much of the underlying functionality for initializing and password encryption/hashing.
    /// </summary>
    public abstract class MembershipProviderBase : MembershipProvider
    {

        public string HashPasswordForStorage(string password)
        {
            string salt;
            var hashed = EncryptOrHashNewPassword(password, out salt);
            return FormatPasswordForStorage(hashed, salt);
        }

        public bool VerifyPassword(string password, string hashedPassword)
        {
            return CheckPassword(password, hashedPassword);
        }

        /// <summary>
        /// Providers can override this setting, default is 7
        /// </summary>
        public virtual int DefaultMinPasswordLength
        {
            get { return 7; }
        }

        /// <summary>
        /// Providers can override this setting, default is 1
        /// </summary>
        public virtual int DefaultMinNonAlphanumericChars
        {
            get { return 1; }
        }

        /// <summary>
        /// Providers can override this setting, default is false to use better security
        /// </summary>
        public virtual bool DefaultUseLegacyEncoding
        {
            get { return false; }
        }

        /// <summary>
        /// Providers can override this setting, by default this is false which means that the provider will 
        /// authenticate the username + password when ChangePassword is called. This property exists purely for
        /// backwards compatibility.
        /// </summary>
        public virtual bool AllowManuallyChangingPassword
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
        private string _customHashAlgorithmType ;
        internal bool UseLegacyEncoding;

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
            _requiresUniqueEmail = config.GetValue("requiresUniqueEmail", true);
            _maxInvalidPasswordAttempts = GetIntValue(config, "maxInvalidPasswordAttempts", 20, false, 0);
            _passwordAttemptWindow = GetIntValue(config, "passwordAttemptWindow", 10, false, 0);
            _minRequiredPasswordLength = GetIntValue(config, "minRequiredPasswordLength", DefaultMinPasswordLength, true, 0x80);
            _minRequiredNonAlphanumericCharacters = GetIntValue(config, "minRequiredNonalphanumericCharacters", DefaultMinNonAlphanumericChars, true, 0x80);
            _passwordStrengthRegularExpression = config["passwordStrengthRegularExpression"];

            _applicationName = config["applicationName"];
            if (string.IsNullOrEmpty(_applicationName))
                _applicationName = GetDefaultAppName();

            //by default we will continue using the legacy encoding.
            UseLegacyEncoding = config.GetValue("useLegacyEncoding", DefaultUseLegacyEncoding);

            // make sure password format is Hashed by default.
            string str = config["passwordFormat"] ?? "Hashed";

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
            {
                var ex = new ProviderException("Provider can not retrieve a hashed password");
                LogHelper.Error<MembershipProviderBase>("Cannot specify a Hashed password format with the enabledPasswordRetrieval option set to true", ex);
                throw ex;
            }
            
            _customHashAlgorithmType = config.GetValue("hashAlgorithmType", string.Empty);
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
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">This property is ignore for this provider</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Checks to ensure the AllowManuallyChangingPassword rule is adhered to
        /// </remarks>       
        public override bool ChangePassword(string username, string oldPassword, string newPassword)
        {
            if (oldPassword.IsNullOrWhiteSpace() && AllowManuallyChangingPassword == false)
            {
                //If the old password is empty and AllowManuallyChangingPassword is false, than this provider cannot just arbitrarily change the password
                throw new NotSupportedException("This provider does not support manually changing the password");
            }

            var args = new ValidatePasswordEventArgs(username, newPassword, false);
            OnValidatingPassword(args);

            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                    throw args.FailureInformation;
                throw new MembershipPasswordException("Change password canceled due to password validation failure.");
            }

            if (AllowManuallyChangingPassword == false)
            {
                if (ValidateUser(username, oldPassword) == false) return false;
            }

            return PerformChangePassword(username, oldPassword, newPassword);
        }

        /// <summary>
        /// Processes a request to update the password for a membership user.
        /// </summary>
        /// <param name="username">The user to update the password for.</param>
        /// <param name="oldPassword">This property is ignore for this provider</param>
        /// <param name="newPassword">The new password for the specified user.</param>
        /// <returns>
        /// true if the password was updated successfully; otherwise, false.
        /// </returns>
        protected abstract bool PerformChangePassword(string username, string oldPassword, string newPassword);

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        /// <remarks>
        /// Performs the basic validation before passing off to PerformChangePasswordQuestionAndAnswer
        /// </remarks>
        public override bool ChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer)
        {
            if (RequiresQuestionAndAnswer == false)
            {
                throw new NotSupportedException("Updating the password Question and Answer is not available if requiresQuestionAndAnswer is not set in web.config");
            }
            
            if (AllowManuallyChangingPassword == false)
            {
                if (ValidateUser(username, password) == false)
                {
                    return false;
                }
            }

            return PerformChangePasswordQuestionAndAnswer(username, password, newPasswordQuestion, newPasswordAnswer);
        }

        /// <summary>
        /// Processes a request to update the password question and answer for a membership user.
        /// </summary>
        /// <param name="username">The user to change the password question and answer for.</param>
        /// <param name="password">The password for the specified user.</param>
        /// <param name="newPasswordQuestion">The new password question for the specified user.</param>
        /// <param name="newPasswordAnswer">The new password answer for the specified user.</param>
        /// <returns>
        /// true if the password question and answer are updated successfully; otherwise, false.
        /// </returns>
        protected abstract bool PerformChangePasswordQuestionAndAnswer(string username, string password, string newPasswordQuestion, string newPasswordAnswer);

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        /// </returns>
        /// <remarks>
        /// Ensures the ValidatingPassword event is executed before executing PerformCreateUser and performs basic membership provider validation of values.
        /// </remarks>
        public override MembershipUser CreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status)
        {
            var valStatus = ValidateNewUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey);
            if (valStatus != MembershipCreateStatus.Success)
            {
                status = valStatus;
                return null;
            }

            return PerformCreateUser(username, password, email, passwordQuestion, passwordAnswer, isApproved, providerUserKey, out status);
        }

        /// <summary>
        /// Performs the validation of the information for creating a new user
        /// </summary>
        /// <param name="username"></param>
        /// <param name="password"></param>
        /// <param name="email"></param>
        /// <param name="passwordQuestion"></param>
        /// <param name="passwordAnswer"></param>
        /// <param name="isApproved"></param>
        /// <param name="providerUserKey"></param>
        /// <returns></returns>
        protected MembershipCreateStatus ValidateNewUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey)
        {
            var args = new ValidatePasswordEventArgs(username, password, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                return MembershipCreateStatus.InvalidPassword;
            }

            // Validate password
            var passwordValidAttempt = IsPasswordValid(password, MinRequiredNonAlphanumericCharacters, PasswordStrengthRegularExpression, MinRequiredPasswordLength);
            if (passwordValidAttempt.Success == false)
            {
                return MembershipCreateStatus.InvalidPassword;
            }

            // Validate email
            if (IsEmailValid(email) == false)
            {
                return MembershipCreateStatus.InvalidEmail;
            }

            // Make sure username isn't all whitespace
            if (string.IsNullOrWhiteSpace(username.Trim()))
            {
                return MembershipCreateStatus.InvalidUserName;
            }

            // Check password question
            if (string.IsNullOrWhiteSpace(passwordQuestion) && RequiresQuestionAndAnswer)
            {
                return MembershipCreateStatus.InvalidQuestion;
            }

            // Check password answer
            if (string.IsNullOrWhiteSpace(passwordAnswer) && RequiresQuestionAndAnswer)
            {
                return MembershipCreateStatus.InvalidAnswer;
            }

            return MembershipCreateStatus.Success;
        }

        /// <summary>
        /// Adds a new membership user to the data source.
        /// </summary>
        /// <param name="username">The user name for the new user.</param>
        /// <param name="password">The password for the new user.</param>
        /// <param name="email">The e-mail address for the new user.</param>
        /// <param name="passwordQuestion">The password question for the new user.</param>
        /// <param name="passwordAnswer">The password answer for the new user</param>
        /// <param name="isApproved">Whether or not the new user is approved to be validated.</param>
        /// <param name="providerUserKey">The unique identifier from the membership data source for the user.</param>
        /// <param name="status">A <see cref="T:System.Web.Security.MembershipCreateStatus"></see> enumeration value indicating whether the user was created successfully.</param>
        /// <returns>
        /// A <see cref="T:System.Web.Security.MembershipUser"></see> object populated with the information for the newly created user.
        /// </returns>
        protected abstract MembershipUser PerformCreateUser(string username, string password, string email, string passwordQuestion, string passwordAnswer, bool isApproved, object providerUserKey, out MembershipCreateStatus status);

        /// <summary>
        /// Gets the members password if password retreival is enabled
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        public override string GetPassword(string username, string answer)
        {
            if (EnablePasswordRetrieval == false)
                throw new ProviderException("Password Retrieval Not Enabled.");

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
                throw new ProviderException("Cannot retrieve Hashed passwords.");

            return PerformGetPassword(username, answer);
        }

        /// <summary>
        /// Gets the members password if password retreival is enabled
        /// </summary>
        /// <param name="username"></param>
        /// <param name="answer"></param>
        /// <returns></returns>
        protected abstract string PerformGetPassword(string username, string answer);

        public override string ResetPassword(string username, string answer)
        {
            if (EnablePasswordReset == false)
            {
                throw new NotSupportedException("Password reset is not supported");
            }

            var newPassword = Membership.GeneratePassword(MinRequiredPasswordLength, MinRequiredNonAlphanumericCharacters);

            var args = new ValidatePasswordEventArgs(username, newPassword, true);
            OnValidatingPassword(args);
            if (args.Cancel)
            {
                if (args.FailureInformation != null)
                {
                    throw args.FailureInformation;
                }
                throw new MembershipPasswordException("Reset password canceled due to password validation failure.");
            }

            return PerformResetPassword(username, answer, newPassword);
        }

        protected abstract string PerformResetPassword(string username, string answer, string generatedPassword);

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

        /// <summary>
        /// If the password format is a hashed keyed algorithm then we will pre-pend the salt used to hash the password
        /// to the hashed password itself.
        /// </summary>
        /// <param name="pass"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        protected internal string FormatPasswordForStorage(string pass, string salt)
        {
            if (UseLegacyEncoding)
            {
                return pass;
            }
            
            if (PasswordFormat == MembershipPasswordFormat.Hashed)
            {
                //the better way, we use salt per member
                return salt + pass;
            }
            return pass;
        }

        internal static bool IsEmailValid(string email)
        {
            const string pattern = @"^(?!\.)(""([^""\r\\]|\\[""\r\\])*""|"
                                   + @"([-a-z0-9!#$%&'*+/=?^_`{|}~]|(?<!\.)\.)*)(?<!\.)"
                                   + @"@[a-z0-9][\w\.-]*[a-z0-9]\.[a-z][a-z\.]*[a-z]$";

            return Regex.IsMatch(email, pattern, RegexOptions.IgnoreCase | RegexOptions.Compiled);
        }

        protected internal string EncryptOrHashPassword(string pass, string salt)
        {
            //if we are doing it the old way

            if (UseLegacyEncoding)
            {
                return LegacyEncodePassword(pass);
            }

            //This is the correct way to implement this (as per the sql membership provider)

            if (PasswordFormat == MembershipPasswordFormat.Clear)
                return pass;
            var bytes = Encoding.Unicode.GetBytes(pass);
            var numArray1 = Convert.FromBase64String(salt);
            byte[] inArray;

            if (PasswordFormat == MembershipPasswordFormat.Hashed)
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
                //this code is copied from the sql membership provider - pretty sure this could be nicely re-written to completely
                // ignore the salt stuff since we are not salting the password when encrypting.
                var password = new byte[numArray1.Length + bytes.Length];
                Buffer.BlockCopy(numArray1, 0, password, 0, numArray1.Length);
                Buffer.BlockCopy(bytes, 0, password, numArray1.Length, bytes.Length);
                inArray = EncryptPassword(password, MembershipPasswordCompatibilityMode.Framework40);
            }
            return Convert.ToBase64String(inArray);
        }

        /// <summary>
        /// Checks the password.
        /// </summary>
        /// <param name="password">The password.</param>
        /// <param name="dbPassword">The dbPassword.</param>
        /// <returns></returns>
        protected internal bool CheckPassword(string password, string dbPassword)
        {
            switch (PasswordFormat)
            {
                case MembershipPasswordFormat.Encrypted:
                    var decrypted = DecryptPassword(dbPassword);
                    return decrypted == password;
                case MembershipPasswordFormat.Hashed:
                    string salt;
                    var storedHashedPass = StoredPassword(dbPassword, out salt);
                    var hashed = EncryptOrHashPassword(password, salt);
                    return storedHashedPass == hashed;
                case MembershipPasswordFormat.Clear:
                    return password == dbPassword;
                default:
                    throw new ArgumentOutOfRangeException();
            }
        }

        /// <summary>
        /// Encrypt/hash a new password with a new salt
        /// </summary>
        /// <param name="newPassword"></param>
        /// <param name="salt"></param>
        /// <returns></returns>
        protected internal string EncryptOrHashNewPassword(string newPassword, out string salt)
        {
            salt = GenerateSalt();
            return EncryptOrHashPassword(newPassword, salt);
        }

        protected internal string DecryptPassword(string pass)
        {
            //if we are doing it the old way

            if (UseLegacyEncoding)
            {
                return LegacyUnEncodePassword(pass);
            }

            //This is the correct way to implement this (as per the sql membership provider)

            switch ((int)PasswordFormat)
            {
                case 0:
                    return pass;
                case 1:
                    throw new ProviderException("Provider can not decrypt hashed password");
                default:
                    var bytes = DecryptPassword(Convert.FromBase64String(pass));
                    return bytes == null ? null : Encoding.Unicode.GetString(bytes, 16, bytes.Length - 16);
            }
        }

        /// <summary>
        /// Returns the hashed password without the salt if it is hashed
        /// </summary>
        /// <param name="storedString"></param>
        /// <param name="salt">returns the salt</param>
        /// <returns></returns>
        internal string StoredPassword(string storedString, out string salt)
        {
            if (UseLegacyEncoding)
            {
                salt = string.Empty;
                return storedString;
            }

            switch (PasswordFormat)
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

        protected internal HashAlgorithm GetHashAlgorithm(string password)
        {
            if (UseLegacyEncoding)
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

            if (_customHashAlgorithmType.IsNullOrWhiteSpace())
            {
                _customHashAlgorithmType = Membership.HashAlgorithmType;
            }

            var alg = HashAlgorithm.Create(_customHashAlgorithmType);
            if (alg == null)
            {
                throw new InvalidOperationException("The hash algorithm specified " + Membership.HashAlgorithmType + " cannot be resolved");
            }

            return alg;
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

        public override string ToString()
        {
            var result = base.ToString();
            var sb = new StringBuilder(result);
            sb.AppendLine("Name =" + Name);
            sb.AppendLine("_applicationName =" + _applicationName);
            sb.AppendLine("_enablePasswordReset=" + _enablePasswordReset);
            sb.AppendLine("_enablePasswordRetrieval=" + _enablePasswordRetrieval);
            sb.AppendLine("_maxInvalidPasswordAttempts=" + _maxInvalidPasswordAttempts);
            sb.AppendLine("_minRequiredNonAlphanumericCharacters=" + _minRequiredNonAlphanumericCharacters);
            sb.AppendLine("_minRequiredPasswordLength=" + _minRequiredPasswordLength);
            sb.AppendLine("_passwordAttemptWindow=" + _passwordAttemptWindow);
            sb.AppendLine("_passwordFormat=" + _passwordFormat);
            sb.AppendLine("_passwordStrengthRegularExpression=" + _passwordStrengthRegularExpression);
            sb.AppendLine("_requiresQuestionAndAnswer=" + _requiresQuestionAndAnswer);
            sb.AppendLine("_requiresUniqueEmail=" + _requiresUniqueEmail);
            return sb.ToString();
        }

    }
}