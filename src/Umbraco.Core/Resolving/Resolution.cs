using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Resolving
{



	// notes: nothing in Resolving is thread-safe because everything should happen when the app is starting

	internal class Resolution
	{
		public static event EventHandler Frozen;

		/// <summary>
		/// Gets a value indicating that resolution is frozen
		/// </summary>
		/// <remarks>
		/// The internal setter is normally used for unit tests
		/// </remarks>
		public static bool IsFrozen { get; internal set; }

		public static void EnsureNotFrozen()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to modify resolvers once resolution is frozen.");
		}

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
