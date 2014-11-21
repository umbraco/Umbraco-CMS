using System;

namespace umbraco.interfaces
{

    [Obsolete("Get rid of this!!")]
	public interface IProperty
	{
		string Alias { get; }
		string Value { get; }
		string ToString();
	}
}