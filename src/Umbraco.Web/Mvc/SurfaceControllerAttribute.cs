using System;

namespace Umbraco.Web.Mvc
{
	/// <summary>
	/// An attribute applied to surface controllers that are not locally declared (i.e. they are shipped as part of a package)
	/// </summary>
	[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
	public class SurfaceControllerAttribute : Attribute
	{
		public Guid Id { get; private set; }

		public SurfaceControllerAttribute(Guid id)
		{
			Id = id;
		}
	}
}