using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Options;
using SixLabors.ImageSharp.Web;
using SixLabors.ImageSharp.Web.Processors;
using Umbraco.Cms.Core.Configuration.Models;
using Umbraco.Cms.Core.Media;

namespace Umbraco.Cms.Web.Common.Media
{
    /// <summary>
    /// Exposes a method that generates an image URL token for request authentication.
    /// </summary>
    /// <seealso cref="IImageUrlTokenGenerator" />
    public class ImageSharpImageUrlTokenGenerator : IImageUrlTokenGenerator
    {
        private readonly byte[]? _hmacSecretKey;
        private readonly Lazy<IList<string>?> _knownCommands;
        private readonly CaseHandlingUriBuilder.CaseHandling _caseHandling;

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="imagingSettings">The Umbraco imaging settings.</param>
        /// <param name="imageWebProcessors">"The image web processors to retrieve the known commands, used to strip out unsupported commands from the generated image URL token.</param>
        public ImageSharpImageUrlTokenGenerator(IOptions<ImagingSettings> imagingSettings, Lazy<IEnumerable<IImageWebProcessor>> imageWebProcessors)
            : this(imagingSettings.Value.HMACSecretKey, imageWebProcessors, CaseHandlingUriBuilder.CaseHandling.LowerInvariant)
        { }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="hmacSecretKey">The HMAC security key.</param>
        /// <param name="knownCommands">"The known commands, used to strip out unsupported commands from the generated image URL token.</param>
        /// <remarks>
        /// This constructor is only used for testing.
        /// </remarks>
        internal ImageSharpImageUrlTokenGenerator(byte[]? hmacSecretKey, IList<string>? knownCommands = null)
            : this(hmacSecretKey, null, CaseHandlingUriBuilder.CaseHandling.LowerInvariant)
        {
            _knownCommands = new Lazy<IList<string>?>(knownCommands);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ImageSharpImageUrlTokenGenerator" /> class.
        /// </summary>
        /// <param name="hmacSecretKey">The HMAC security key.</param>
        /// <param name="imageWebProcessors">"The image web processors to retrieve the known commands, used to strip out unsupported commands from the generated image URL token.</param>
        /// <param name="caseHandling">Determines case handling for the result.</param>
        protected ImageSharpImageUrlTokenGenerator(byte[]? hmacSecretKey, Lazy<IEnumerable<IImageWebProcessor>>? imageWebProcessors, CaseHandlingUriBuilder.CaseHandling caseHandling)
        {
            _hmacSecretKey = hmacSecretKey;
            _knownCommands = new Lazy<IList<string>?>(() => imageWebProcessors?.Value.SelectMany(x => x.Commands).Distinct().ToList());
            _caseHandling = caseHandling;
        }

        /// <inheritdoc />
        public string? GetImageUrlToken(string imageUrl, IEnumerable<KeyValuePair<string, string?>> commands)
        {
            if (_hmacSecretKey == null || _hmacSecretKey.Length == 0)
            {
                return null;
            }

            var queryString = QueryString.Create(StripCommands(commands));
            string value = CaseHandlingUriBuilder.BuildRelative(_caseHandling, null, imageUrl, queryString);

            return ComputeImageUrlToken(value, _hmacSecretKey);
        }

        /// <summary>
        /// Strips out any commands from being included in the image URL token.
        /// </summary>
        /// <param name="commands">The commands.</param>
        /// <returns>
        /// The commands used when generating and validating the image URL token.
        /// </returns>
        /// <remarks>
        /// Unknown commands will already be removed by ImageSharp when generating the image URL token, so this ensures it's also done when validating.
        /// This can be overwritten to strip out additional commands, enabling a way to whitelist specific commands.
        /// </remarks>
        protected virtual IEnumerable<KeyValuePair<string, string?>> StripCommands(IEnumerable<KeyValuePair<string, string?>> commands)
        {
            if (_knownCommands.Value is IList<string> knownCommands)
            {
                return commands.Where(x => knownCommands.Contains(x.Key));
            }

            return commands;
        }

        /// <summary>
        /// Computes the image URL token by hashing the <paramref name="value" /> using the specified <paramref name="secret" />.
        /// </summary>
        /// <param name="value">The value to hash.</param>
        /// <param name="secret">The secret key.</param>
        /// <returns>
        /// The computed image URL token.
        /// </returns>
        /// <remarks>
        /// This is used for both generating and validating the image URL token, therefore can be overwritten to change the hashing algorithm.
        /// </remarks>
        protected virtual string ComputeImageUrlToken(string value, byte[] secret)
            => HMACUtilities.ComputeHMACSHA256(value, secret);
    }
}
