using System;
using System.Globalization;

namespace Umbraco.Core.Dictionary
{
	/// <summary>
	/// Represents a dictionary based on a specific culture
	/// </summary>
	public interface ICultureDictionary
	{
		/// <summary>
		/// Returns the dictionary value based on the key supplied
		/// </summary>
		/// <param name="key"></param>
		/// <returns></returns>
		string this[string key] { get; }

		/// <summary>
		/// Returns the current culture
		/// </summary>
		CultureInfo Culture { get; }
	}
}