using System;

namespace Umbraco.Core
{
	public static class IntExtensions
	{
		/// <summary>
		/// Does something 'x' amount of times
		/// </summary>
		/// <param name="n"></param>
		/// <param name="action"></param>
		public static void Times(this int n, Action<int> action)
		{
			for (int i = 0; i < n; i++)
			{
				action(i);
			}
		}
	}
}