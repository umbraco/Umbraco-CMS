using System.Security.Cryptography;
using Umbraco.Cms.Core.Configuration;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Generates a password
/// </summary>
/// <remarks>
///     This uses logic copied from the old MembershipProvider.GeneratePassword logic
/// </remarks>
public class PasswordGenerator
{
    private readonly IPasswordConfiguration _passwordConfiguration;

    public PasswordGenerator(IPasswordConfiguration passwordConfiguration) =>
        _passwordConfiguration = passwordConfiguration;

    public string GeneratePassword()
    {
        var password = PasswordStore.GeneratePassword(
            _passwordConfiguration.RequiredLength,
            _passwordConfiguration.GetMinNonAlphaNumericChars());

        var passwordChars = password.ToCharArray();

        if (_passwordConfiguration.RequireDigit &&
            passwordChars.ContainsAny(Enumerable.Range(48, 58).Select(x => (char)x)))
        {
            password += Convert.ToChar(RandomNumberGenerator.GetInt32(48, 58)); // 0-9
        }

        if (_passwordConfiguration.RequireLowercase &&
            passwordChars.ContainsAny(Enumerable.Range(97, 123).Select(x => (char)x)))
        {
            password += Convert.ToChar(RandomNumberGenerator.GetInt32(97, 123)); // a-z
        }

        if (_passwordConfiguration.RequireUppercase &&
            passwordChars.ContainsAny(Enumerable.Range(65, 91).Select(x => (char)x)))
        {
            password += Convert.ToChar(RandomNumberGenerator.GetInt32(65, 91)); // A-Z
        }

        if (_passwordConfiguration.RequireNonLetterOrDigit &&
            passwordChars.ContainsAny(Enumerable.Range(33, 48).Select(x => (char)x)))
        {
            password += Convert.ToChar(RandomNumberGenerator.GetInt32(33, 48)); // symbols !"#$%&'()*+,-./
        }

        return password;
    }

    /// <summary>
    ///     Internal class copied from ASP.NET Framework MembershipProvider
    /// </summary>
    /// <remarks>
    ///     See https://stackoverflow.com/a/39855417/694494 +
    ///     https://github.com/Microsoft/referencesource/blob/master/System.Web/Security/Membership.cs
    /// </remarks>
    private static class PasswordStore
    {
        private static readonly char[] Punctuations = "!@#$%^&*()_-+=[{]};:>|./?".ToCharArray();
        private static readonly char[] StartingChars = { '<', '&' };

        /// <summary>Generates a random password of the specified length.</summary>
        /// <returns>A random password of the specified length.</returns>
        /// <param name="length">
        ///     The number of characters in the generated password. The length must be between 1 and 128
        ///     characters.
        /// </param>
        /// <param name="numberOfNonAlphanumericCharacters">
        ///     The minimum number of non-alphanumeric characters (such as @, #, !, %,
        ///     &amp;, and so on) in the generated password.
        /// </param>
        /// <exception cref="T:System.ArgumentException">
        ///     <paramref name="length" /> is less than 1 or greater than 128 -or-
        ///     <paramref name="numberOfNonAlphanumericCharacters" /> is less than 0 or greater than <paramref name="length" />.
        /// </exception>
        public static string GeneratePassword(int length, int numberOfNonAlphanumericCharacters)
        {
            if (length < 1 || length > 128)
            {
                throw new ArgumentException("password length incorrect", nameof(length));
            }

            if (numberOfNonAlphanumericCharacters > length || numberOfNonAlphanumericCharacters < 0)
            {
                throw new ArgumentException(
                    "min required non alphanumeric characters incorrect",
                    nameof(numberOfNonAlphanumericCharacters));
            }

            string s;
            do
            {
                var data = new byte[length];
                var chArray = new char[length];
                var num1 = 0;
                new RNGCryptoServiceProvider().GetBytes(data);

                for (var index = 0; index < length; ++index)
                {
                    var num2 = data[index] % 87;
                    if (num2 < 10)
                    {
                        chArray[index] = (char)(48 + num2);
                    }
                    else if (num2 < 36)
                    {
                        chArray[index] = (char)(65 + num2 - 10);
                    }
                    else if (num2 < 62)
                    {
                        chArray[index] = (char)(97 + num2 - 36);
                    }
                    else
                    {
                        chArray[index] = Punctuations[num2 - 62];
                        ++num1;
                    }
                }

                if (num1 < numberOfNonAlphanumericCharacters)
                {

                    for (var index1 = 0; index1 < numberOfNonAlphanumericCharacters - num1; ++index1)
                    {
                        int index2;
                        do
                        {
                            index2 = RandomNumberGenerator.GetInt32(0, length);
                        }
                        while (!char.IsLetterOrDigit(chArray[index2]));

                        chArray[index2] = Punctuations[RandomNumberGenerator.GetInt32(0, Punctuations.Length)];
                    }
                }

                s = new string(chArray);
            }
            while (IsDangerousString(s, out int matchIndex));

            return s;
        }

        private static bool IsDangerousString(string s, out int matchIndex)
        {
            // bool inComment = false;
            matchIndex = 0;

            for (var i = 0; ;)
            {
                // Look for the start of one of our patterns
                var n = s.IndexOfAny(StartingChars, i);

                // If not found, the string is safe
                if (n < 0)
                {
                    return false;
                }

                // If it's the last char, it's safe
                if (n == s.Length - 1)
                {
                    return false;
                }

                matchIndex = n;

                switch (s[n])
                {
                    case '<':
                        // If the < is followed by a letter or '!', it's unsafe (looks like a tag or HTML comment)
                        if (IsAtoZ(s[n + 1]) || s[n + 1] == '!' || s[n + 1] == '/' || s[n + 1] == '?')
                        {
                            return true;
                        }

                        break;
                    case '&':
                        // If the & is followed by a #, it's unsafe (e.g. &#83;)
                        if (s[n + 1] == '#')
                        {
                            return true;
                        }

                        break;
                }

                // Continue searching
                i = n + 1;
            }
        }

        private static bool IsAtoZ(char c)
        {
            if (c >= 97 && c <= 122)
            {
                return true;
            }

            if (c >= 65)
            {
                return c <= 90;
            }

            return false;
        }
    }
}
