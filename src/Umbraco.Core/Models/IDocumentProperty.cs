using System;

namespace Umbraco.Core.Models
{
	public interface IDocumentProperty
	{
		string Alias { get; }
		object Value { get; }
		Guid Version { get; }
	}
}