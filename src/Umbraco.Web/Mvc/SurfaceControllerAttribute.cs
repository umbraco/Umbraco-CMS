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

		public SurfaceControllerAttribute(string id)
		{
			Guid gid;
			if (Guid.TryParse(id, out gid))
			{
				Id = gid;
			}
			else
			{
				throw new InvalidCastException("Cannot convert the value " + id + " to a Guid");	
			}
		}
	}
}