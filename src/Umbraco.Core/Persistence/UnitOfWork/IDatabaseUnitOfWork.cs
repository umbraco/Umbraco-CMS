using System;

namespace Umbraco.Core.Persistence.UnitOfWork
{
	/// <summary>
	/// Defines a unit of work when working with a database object
	/// </summary>
	public interface IDatabaseUnitOfWork : IUnitOfWork, IDisposable
	{
		UmbracoDatabase Database { get; }
	}
}