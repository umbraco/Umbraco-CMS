using System.Globalization;

namespace Umbraco.Core
{
	/// <summary>
	/// Represents a dictionary based on a specific culture
	/// </summary>
	internal interface ICultureDictionary
	{
		string this[string key] { get; }
		CultureInfo Language { get; }
	}
}