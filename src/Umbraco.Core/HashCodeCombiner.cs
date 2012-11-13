using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text;

namespace Umbraco.Core
{
	/// <summary>
	/// Used to create a hash code from multiple objects.
	/// </summary>
	/// <remarks>
	/// .Net has a class the same as this: System.Web.Util.HashCodeCombiner and of course it works for all sorts of things
	/// and is probably more stable in general, however we just need a quick easy class for this in order to create a unique
	/// hash of plugins to see if they've changed.
	/// </remarks>
	internal class HashCodeCombiner
	{
		private int _combinedHash;

		internal void AddInt(int i)
		{
			_combinedHash = ((_combinedHash << 5) + _combinedHash) ^ i;
		}

		internal void AddObject(object o)
		{
			AddInt(o.GetHashCode());
		}

		internal void AddDateTime(DateTime d)
		{
			AddInt(d.GetHashCode());
		}

		internal void AddCaseInsensitiveString(string s)
		{
			if (s != null)
				AddInt((StringComparer.InvariantCultureIgnoreCase).GetHashCode(s));
		}

		internal void AddFile(FileInfo f)
		{
			AddCaseInsensitiveString(f.FullName);
			AddDateTime(f.CreationTimeUtc);
			AddDateTime(f.LastWriteTimeUtc);
			AddInt(f.Length.GetHashCode());
		}

		internal void AddFolder(DirectoryInfo d)
		{
			AddCaseInsensitiveString(d.FullName);
			AddDateTime(d.CreationTimeUtc);
			AddDateTime(d.LastWriteTimeUtc);
			foreach (var f in d.GetFiles())
			{
				AddFile(f);
			}
			foreach (var s in d.GetDirectories())
			{
				AddFolder(s);
			}
		}

		/// <summary>
		/// Returns the hex code of the combined hash code
		/// </summary>
		/// <returns></returns>
		internal string GetCombinedHashCode()
		{
			return _combinedHash.ToString("x", CultureInfo.InvariantCulture);
		}

	}
}
