using System;

namespace Umbraco.Core.Models
{
	public interface IPublishedContentProperty
	{
		string Alias { get; }
		object Value { get; }		
	}
}