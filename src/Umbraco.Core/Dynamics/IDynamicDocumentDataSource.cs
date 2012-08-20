using System;
using System.Collections.Generic;

namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// This exists only because we want Dynamics in the Core project but DynamicNode has references to ContentType to run some queries
	/// and currently the business logic part of Umbraco is still in the legacy project and we don't want to move that to the core so in the
	/// meantime until the new APIs are made, we need to have this data source in place with a resolver which is set in the web project.
	/// </summary>
	internal interface IDynamicDocumentDataSource
	{
		Guid GetDataType(string docTypeAlias, string propertyAlias);
	}
}