using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// A resolver used to return the current implementation of the RepositoryInstanceFactory
	/// </summary>
	internal class RepositoryInstanceResolver : SingleObjectResolverBase<RepositoryInstanceResolver,RepositoryInstanceFactory>
	{
		internal RepositoryInstanceResolver(RepositoryInstanceFactory registrar)
			: base(registrar)
		{
		} 

		/// <summary>
		/// Return the repository based on the type
		/// </summary>
		/// <typeparam name="TRepository"></typeparam>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		internal TRepository ResolveByType<TRepository>(IUnitOfWork unitOfWork)
		{
			//TODO: REMOVE all of these binding flags once the IDictionaryRepository, IMacroRepository are public! As this probably
			// wont work in medium trust!
			var createMethod = this.Value.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public) 
				.First(x => x.GetCustomAttributes(true).OfType<RepositoryInstanceTypeAttribute>()
					            .Any(instance => instance.InterfaceType.IsType<TRepository>()));
			if (createMethod.GetParameters().Count() != 1
			    || !createMethod.GetParameters().Single().ParameterType.IsType<IUnitOfWork>())
			{
				throw new FormatException("The method " + createMethod.Name + " must only contain one parameter of type " + typeof(IUnitOfWork).FullName);
			}
			if (!createMethod.ReturnType.IsType<TRepository>())
			{
				throw new FormatException("The method " + createMethod.Name + " must return the type " + typeof(TRepository).FullName);
			}

			return (TRepository) createMethod.Invoke(this.Value, new object[] {unitOfWork});
		}

	}
}