using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Umbraco.Core.Resolving
{
	// notes: nothing in Resolving is thread-safe because everything should happen when the app is starting

	public class Resolution
	{
		public static event EventHandler Freezing;
		public static event EventHandler Frozen;

		public static bool IsFrozen { get; private set; }

		public static void EnsureNotFrozen()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to modify resolvers once resolution is frozen.");
		}

		public static void Freeze()
		{
			if (Resolution.IsFrozen)
				throw new InvalidOperationException("Resolution is frozen. It is not possible to freeze it again.");
			if (Freezing != null)
				Freezing(null, null);
			IsFrozen = true;
			if (Frozen != null)
				Frozen(null, null);
		}
	}
}
