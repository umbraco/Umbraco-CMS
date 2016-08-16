// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RelatedLink.cs" company="Umbraco">
//   Umbraco
// </copyright>
// <summary>
//   Defines the RelatedLink type.
// </summary>
// --------------------------------------------------------------------------------------------------------------------

﻿namespace Umbraco.Web.Models
{
	public class RelatedLink : RelatedLinkBase
	{
		public int? Id { get; internal set; }
		internal bool IsDeleted { get; set; }
	}
}
