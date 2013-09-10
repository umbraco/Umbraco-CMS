using System;

namespace umbraco.interfaces
{
	public interface IProperty
	{
		string Alias { get; }
		string Value { get; }
		string ToString();
	}
}