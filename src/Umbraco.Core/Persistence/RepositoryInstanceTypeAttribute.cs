using System;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// Used to decorate each method of the class RepositoryInstanceFactory (or derived classes) to inform the system of what
	/// type of Repository the method will be returning.
	/// </summary>
	/// <remarks>
	/// The reason for this attribute is because the RepositoryInstanceFactory (or derived classes) might contain methods multiple
	/// methods that return the interface repository being asked for so we cannot simply rely on the return type of the methods.
	/// </remarks>
	[AttributeUsage(AttributeTargets.Method)]
	public class RepositoryInstanceTypeAttribute : Attribute
	{
		public Type InterfaceType { get; private set; }

		public RepositoryInstanceTypeAttribute(Type interfaceType)
		{
			InterfaceType = interfaceType;
		}
	}
}