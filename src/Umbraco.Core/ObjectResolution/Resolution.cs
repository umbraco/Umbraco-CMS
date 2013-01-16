using System;

namespace Umbraco.Core.ObjectResolution
{
	/// <summary>
	/// Represents the status of objects resolution.
	/// </summary>
	/// <remarks>
	/// <para>Objects resolution can be frozen ie nothing can change anymore.</para>
	/// <para>Nothing in resolution is thread-safe, because everything should take place when the application is starting.</para>
	/// </remarks>
	internal class Resolution
	{
		// NOTE : must clarify freezing... SingleObjectResolverBase does not honor it...

		/// <summary>
		/// Occurs when resolution is frozen.
		/// </summary>
		/// <remarks>Occurs only once, since resolution can be frozen only once.</remarks>
		public static event EventHandler Frozen;

		/// <summary>
		/// Gets or sets a value indicating whether resolution of objects is frozen.
		/// </summary>
		/// <remarks>The internal setter is to be used in unit tests.</remarks>
		public static bool IsFrozen { get; internal set; }

		/// <summary>
		/// Ensures that resolution is not frozen, else throws.
		/// </summary>
		/// <exception cref="InvalidOperationException">resolution is frozen.</exception>
		public static void EnsureNotFrozen()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to modify resolvers once resolution is frozen.");
		}

		/// <summary>
		/// Freezes resolution.
		/// </summary>
		/// <exception cref="InvalidOperationException">resolution is already frozen.</exception>
		public static void Freeze()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to freeze it again.");

			IsFrozen = true;
			if (Frozen != null)
				Frozen(null, null);
		}
	}
}
