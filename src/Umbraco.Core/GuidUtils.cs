using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace Umbraco.Core
{
    /// <summary>
    /// Utility methods for the <see cref="Guid"/> struct.
    /// </summary>
    internal static class GuidUtils
    {
        /// <summary>
        /// Combines two guid instances utilizing an exclusive disjunction.
        /// The resultant guid is not guaranteed to be unique since the number of unique bits is halved.
        /// </summary>
        /// <param name="a">The first guid.</param>
        /// <param name="b">The seconds guid.</param>
        /// <returns></returns>
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Guid Combine(Guid a, Guid b)
        {
            var ad = new DecomposedGuid(a);
            var bd = new DecomposedGuid(b);

            ad.Hi ^= bd.Hi;
            ad.Lo ^= bd.Lo;

            return ad.Value;
        }

        /// <summary>
        /// A decomposed guid. Allows access to the high and low bits without unsafe code.
        /// </summary>
        [StructLayout(LayoutKind.Explicit)]
        private struct DecomposedGuid
        {
            [FieldOffset(00)] public Guid Value;
            [FieldOffset(00)] public long Hi;
            [FieldOffset(08)] public long Lo;

            public DecomposedGuid(Guid value) : this() => this.Value = value;
        }
    }
}
