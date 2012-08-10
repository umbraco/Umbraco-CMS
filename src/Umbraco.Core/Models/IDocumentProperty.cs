using System;

namespace Umbraco.Core.Models
{
	public interface IDocumentProperty
	{
		string Alias { get; }
		string Value { get; }
		Guid Version { get; }
	}
}