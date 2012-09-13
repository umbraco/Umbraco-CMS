using System.Xml;
using Umbraco.Core.Models;
using umbraco;
using umbraco.interfaces;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// Represents the model for the current rendering page in Umbraco
	/// </summary>
	public class RenderModel
	{
		//NOTE: the model isn't just IDocument because in the future we will most likely want to add other properties here, 
		//or we will want to add extensions.

		public IDocument CurrentDocument { get; set; }
	}
}