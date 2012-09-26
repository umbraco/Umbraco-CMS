using System;
using System.Linq;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// An attribute applied to a plugin controller that requires that it is routed to its own area
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class PluginControllerAttribute : Attribute
	{
		public string AreaName { get; private set; }

		public PluginControllerAttribute(string areaName)
		{
			//validate this, only letters and digits allowed.
			if (areaName.Any(c => !Char.IsLetterOrDigit(c)))
			{
				throw new FormatException("The areaName specified " + areaName + " can only contains letters and digits");
			}

			AreaName = areaName;
		}

	}
}