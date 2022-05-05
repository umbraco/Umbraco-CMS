using System;
using Microsoft.AspNetCore.DataProtection.Infrastructure;
using Umbraco.Extensions;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Contains extension methods for the <see cref="IApplicationDiscriminator" /> interface.
    /// </summary>
    public static class ApplicationDiscriminatorExtensions
    {
        private static string? s_applicationId;

        /// <summary>
        /// Gets an application id which respects downstream customizations.
        /// </summary>
        /// <remarks>
        /// Hashed to obscure any unintended infrastructure details e.g. the default value is ContentRootPath.
        /// </remarks>
        public static string? GetApplicationId(this IApplicationDiscriminator applicationDiscriminator)
        {
            if (s_applicationId != null)
            {
                return s_applicationId;
            }

            if (applicationDiscriminator == null)
            {
                throw new ArgumentNullException(nameof(applicationDiscriminator));
            }

            return s_applicationId = applicationDiscriminator.Discriminator?.GenerateHash();
        }
    }
}
