using System.ComponentModel;
using System.Security.Cryptography;
using System.Text;
using Umbraco.Extensions;

namespace Umbraco.Cms.Core.Security;

/// <summary>
///     Handles password hashing and formatting for legacy hashing algorithms.
/// </summary>
/// <remarks>
///     Should probably be internal.
/// </remarks>
public class LegacyPasswordSecurity
{
    public static string GenerateSalt()
    {
        var numArray = new byte[16];
        new RNGCryptoServiceProvider().GetBytes(numArray);
        return Convert.ToBase64String(numArray);
    }

    // TODO: Remove v11
    // Used for tests
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("We shouldn't be altering our public API to make test code easier, removing v11")]
    public string HashPasswordForStorage(string algorithmType, string password)
    {
        if (string.IsNullOrWhiteSpace(password))
        {
            throw new ArgumentException("password cannot be empty", nameof(password));
        }

        var hashed = HashNewPassword(algorithmType, password, out string salt);
        return FormatPasswordForStorage(algorithmType, hashed, salt);
    }

    // TODO: Remove v11
    // Used for tests
    [EditorBrowsable(EditorBrowsableState.Never)]
    [Obsolete("We shouldn't be altering our public API to make test code easier, removing v11")]
    public string FormatPasswordForStorage(string algorithmType, string hashedPassword, string salt)
    {
        if (!SupportHashAlgorithm(algorithmType))
        {
            throw new InvalidOperationException($"{algorithmType} is not supported");
        }

        return salt + hashedPassword;
    }

    /// <summary>
    ///     Verifies if the password matches the expected hash+salt of the stored password string
    /// </summary>
    /// <param name="algorithm">The hashing algorithm for the stored password.</param>
    /// <param name="password">The password.</param>
    /// <param name="dbPassword">The value of the password stored in a data store.</param>
    /// <returns></returns>
    public bool VerifyPassword(string algorithm, string password, string dbPassword)
    {
        if (string.IsNullOrWhiteSpace(dbPassword))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(dbPassword));
        }

        if (dbPassword.StartsWith(Constants.Security.EmptyPasswordPrefix))
        {
            return false;
        }

        try
        {
            var storedHashedPass = ParseStoredHashPassword(algorithm, dbPassword, out var salt);
            var hashed = HashPassword(algorithm, password, salt);
            return storedHashedPass == hashed;
        }
        catch (ArgumentOutOfRangeException)
        {
            // This can happen if the length of the password is wrong and a salt cannot be extracted.
            return false;
        }
    }

    /// <summary>
    ///     Verify a legacy hashed password (HMACSHA1)
    /// </summary>
    public bool VerifyLegacyHashedPassword(string password, string dbPassword)
    {
        var hashAlgorithm = new HMACSHA1
        {
            // the legacy salt was actually the password :(
            Key = Encoding.Unicode.GetBytes(password),
        };

        var hashed = Convert.ToBase64String(hashAlgorithm.ComputeHash(Encoding.Unicode.GetBytes(password)));

        return dbPassword == hashed;
    }

    /// <summary>
    ///     Create a new password hash and a new salt
    /// </summary>
    /// <param name="algorithm">The hashing algorithm for the password.</param>
    /// <param name="newPassword"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    // TODO: Do we need this method? We shouldn't be using this class to create new password hashes for storage
    // TODO: Remove v11
    [Obsolete("We shouldn't be altering our public API to make test code easier, removing v11")]
    public string HashNewPassword(string algorithm, string newPassword, out string salt)
    {
        salt = GenerateSalt();
        return HashPassword(algorithm, newPassword, salt);
    }

    /// <summary>
    ///     Parses out the hashed password and the salt from the stored password string value
    /// </summary>
    /// <param name="algorithm">The hashing algorithm for the stored password.</param>
    /// <param name="storedString"></param>
    /// <param name="salt">returns the salt</param>
    /// <returns></returns>
    public string ParseStoredHashPassword(string algorithm, string storedString, out string salt)
    {
        if (string.IsNullOrWhiteSpace(storedString))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(storedString));
        }

        if (!SupportHashAlgorithm(algorithm))
        {
            throw new InvalidOperationException($"{algorithm} is not supported");
        }

        var saltLen = GenerateSalt();
        salt = storedString.Substring(0, saltLen.Length);
        return storedString.Substring(saltLen.Length);
    }

    public bool SupportHashAlgorithm(string algorithm)
    {
        // This is for the v6-v8 hashing algorithm
        if (algorithm.InvariantEquals(Constants.Security.AspNetUmbraco8PasswordHashAlgorithmName))
        {
            return true;
        }

        // Default validation value for old machine keys (switched to HMACSHA256 aspnet 4 https://docs.microsoft.com/en-us/aspnet/whitepapers/aspnet4/breaking-changes)
        if (algorithm.InvariantEquals("SHA1"))
        {
            return true;
        }

        return false;
    }

    /// <summary>
    ///     Hashes a password with a given salt
    /// </summary>
    /// <param name="algorithmType">The hashing algorithm for the password.</param>
    /// <param name="pass"></param>
    /// <param name="salt"></param>
    /// <returns></returns>
    private string HashPassword(string algorithmType, string pass, string salt)
    {
        if (!SupportHashAlgorithm(algorithmType))
        {
            throw new InvalidOperationException($"{algorithmType} is not supported");
        }

        // This is the correct way to implement this (as per the sql membership provider)
        var bytes = Encoding.Unicode.GetBytes(pass);
        var saltBytes = Convert.FromBase64String(salt);
        byte[] inArray;

        using HashAlgorithm hashAlgorithm = GetHashAlgorithm(algorithmType);
        if (hashAlgorithm is KeyedHashAlgorithm algorithm)
        {
            KeyedHashAlgorithm keyedHashAlgorithm = algorithm;
            if (keyedHashAlgorithm.Key.Length == saltBytes.Length)
            {
                // if the salt bytes is the required key length for the algorithm, use it as-is
                keyedHashAlgorithm.Key = saltBytes;
            }
            else if (keyedHashAlgorithm.Key.Length < saltBytes.Length)
            {
                // if the salt bytes is too long for the required key length for the algorithm, reduce it
                var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                Buffer.BlockCopy(saltBytes, 0, numArray2, 0, numArray2.Length);
                keyedHashAlgorithm.Key = numArray2;
            }
            else
            {
                // if the salt bytes is too short for the required key length for the algorithm, extend it
                var numArray2 = new byte[keyedHashAlgorithm.Key.Length];
                var dstOffset = 0;
                while (dstOffset < numArray2.Length)
                {
                    var count = Math.Min(saltBytes.Length, numArray2.Length - dstOffset);
                    Buffer.BlockCopy(saltBytes, 0, numArray2, dstOffset, count);
                    dstOffset += count;
                }

                keyedHashAlgorithm.Key = numArray2;
            }

            inArray = keyedHashAlgorithm.ComputeHash(bytes);
        }
        else
        {
            var buffer = new byte[saltBytes.Length + bytes.Length];
            Buffer.BlockCopy(saltBytes, 0, buffer, 0, saltBytes.Length);
            Buffer.BlockCopy(bytes, 0, buffer, saltBytes.Length, bytes.Length);
            inArray = hashAlgorithm.ComputeHash(buffer);
        }

        return Convert.ToBase64String(inArray);
    }

    /// <summary>
    ///     Return the hash algorithm to use based on the <see cref="IPasswordConfiguration" />
    /// </summary>
    /// <param name="algorithm">The hashing algorithm name.</param>
    /// <param name="password"></param>
    /// <returns></returns>
    private HashAlgorithm GetHashAlgorithm(string algorithm)
    {
        if (algorithm.IsNullOrWhiteSpace())
        {
            throw new InvalidOperationException("No hash algorithm type specified");
        }

        var alg = HashAlgorithm.Create(algorithm);
        if (alg == null)
        {
            throw new InvalidOperationException($"The hash algorithm specified {algorithm} cannot be resolved");
        }

        return alg;
    }
}
