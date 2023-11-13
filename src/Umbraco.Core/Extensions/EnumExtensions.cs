// Copyright (c) Umbraco.
// See LICENSE for more details.

using System;
using Umbraco.Cms.Core.Services.AuthorizationStatus;
using Umbraco.Cms.Core.Services.OperationStatus;

namespace Umbraco.Extensions
{
    /// <summary>
    /// Provides extension methods to <see cref="Enum"/>.
    /// </summary>
    public static class EnumExtensions
    {
        /// <summary>
        /// Determines whether any of the flags/bits are set within the enum value.
        /// </summary>
        /// <typeparam name="T">The enum type.</typeparam>
        /// <param name="value">The value.</param>
        /// <param name="flags">The flags.</param>
        /// <returns>
        ///   <c>true</c> if any of the flags/bits are set within the enum value; otherwise, <c>false</c>.
        /// </returns>
        public static bool HasFlagAny<T>(this T value, T flags)
            where T : Enum
        {
            var v = Convert.ToUInt64(value);
            var f = Convert.ToUInt64(flags);

            return (v & f) > 0;
        }

        /// <summary>
        ///     Converts from <see cref="UserGroupAuthorizationStatus" /> to <see cref="UserGroupOperationStatus" />.
        /// </summary>
        /// <param name="from">The authorization status to convert from.</param>
        /// <returns>The corresponding operation status.</returns>
        /// <exception cref="NotImplementedException">Thrown if an authorization status does not have a corresponding operation status.</exception>
        internal static UserGroupOperationStatus ToUserGroupOperationStatus(this UserGroupAuthorizationStatus from) =>
            from switch
            {
                UserGroupAuthorizationStatus.Success
                    => UserGroupOperationStatus.Success,
                UserGroupAuthorizationStatus.UnauthorizedMissingAllowedSectionAccess
                    => UserGroupOperationStatus.UnauthorizedMissingAllowedSectionAccess,
                UserGroupAuthorizationStatus.UnauthorizedMissingContentStartNodeAccess
                    => UserGroupOperationStatus.UnauthorizedMissingContentStartNodeAccess,
                UserGroupAuthorizationStatus.UnauthorizedMissingMediaStartNodeAccess
                    => UserGroupOperationStatus.UnauthorizedMissingMediaStartNodeAccess,
                UserGroupAuthorizationStatus.UnauthorizedMissingUserGroupAccess
                    => UserGroupOperationStatus.UnauthorizedMissingUserGroupAccess,
                UserGroupAuthorizationStatus.UnauthorizedMissingUserSectionAccess
                    => UserGroupOperationStatus.UnauthorizedMissingUserSectionAccess,
                _ => throw new NotImplementedException("UserGroupAuthorizationStatus does not map to a corresponding UserGroupOperationStatus")
            };
    }
}
