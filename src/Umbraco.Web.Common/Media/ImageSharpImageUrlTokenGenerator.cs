using System.Collections.Generic;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Web.Common.Media
{
    /// <summary>
    /// Exposes a method that generates an image URL token for request authentication based .
    /// </summary>
    /// <seealso cref="IImageUrlTokenGenerator" />
    public class ImageSharpImageUrlTokenGenerator : IImageUrlTokenGenerator
    {
        private readonly byte[]? _hmacSecretKey;
        private readonly CaseHandlingUriBuilder.CaseHandling _caseHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="imagingSettings">The Umbraco imaging settings.</param>
        public ImageSharpImageUrlTokenGenerator(IOptions<ImagingSettings> imagingSettings)
            : this(imagingSettings.Value.HMACSecretKey, CaseHandlingUriBuilder.CaseHandling.LowerInvariant)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="hmacSecretKey">The HMAC security key.</param>
        /// <remarks>
        /// This constructor is only used for testing.
        /// </remarks>
        internal ImageSharpImageUrlTokenGenerator(byte[]? hmacSecretKey)
            : this(hmacSecretKey, CaseHandlingUriBuilder.CaseHandling.LowerInvariant)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="hmacSecretKey">The HMAC security key.</param>
        /// <param name="caseHandling">Determines case handling for the result.</param>
        protected ImageSharpImageUrlTokenGenerator(byte[]? hmacSecretKey, CaseHandlingUriBuilder.CaseHandling caseHandling)
        {
            _hmacSecretKey = hmacSecretKey;
            _caseHandling = caseHandling;
        }

        /// <inheritdoc />
        public string? GetImageUrlToken(string imageUrl, IEnumerable<KeyValuePair<string, string?>> commands)
        {
            if (_hmacSecretKey == null || _hmacSecretKey.Length == 0)
            {
                return null;
            }

            QueryString queryString = CreateImageUrlTokenQueryString(commands);
            string value = CaseHandlingUriBuilder.BuildRelative(_caseHandling, null, imageUrl, queryString);

            return ComputeImageUrlToken(value, _hmacSecretKey);
        }

        /// <summary>
        /// Creates the query string that is included in the image URL token.
        /// </summary>
        /// <param name="commands">The commands to include.</param>
        /// <returns>
        /// The query string.
        /// </returns>
        /// <remarks>
        /// The ImageSharp middleware computes the token using only known commands,
        /// so if you've removed default processors or add unknown commands using the FurtherOptions,
        /// you can use this method to filter out any unsupported/unknown commands.
        /// </remarks>
        protected virtual QueryString CreateImageUrlTokenQueryString(IEnumerable<KeyValuePair<string, string?>> commands)
            => QueryString.Create(commands);

        /// <summary>
        /// Computes the image URL token by hashing the <paramref name="value" /> using the specified <paramref name="secret" />.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="secret">The secret key.</param>
        /// <returns>
        /// The computed image URL token.
        /// </returns>
        protected virtual string ComputeImageUrlToken(string value, byte[] secret)
            => HMACUtilities.ComputeHMACSHA256(value, secret);
    }
}
