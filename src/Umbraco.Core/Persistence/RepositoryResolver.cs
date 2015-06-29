using System;
using System.Linq;
using System.Reflection;
using Umbraco.Core.ObjectResolution;
using Umbraco.Core.Persistence.Repositories;
using Umbraco.Core.Persistence.UnitOfWork;

namespace Umbraco.Core.Persistence
{
	/// <summary>
	/// A resolver used to return the current implementation of the RepositoryInstanceFactory
	/// </summary>
	internal class RepositoryResolver : SingleObjectResolverBase<RepositoryResolver, RepositoryFactory>
	{
		internal RepositoryResolver(RepositoryFactory registrar)
			: base(registrar)
		{
		}

		/// <summary>
		/// Can be used by developers at runtime to set their own RepositoryInstanceFactory at app startup
		/// </summary>
		/// <param name="factory"></param>
		public void SetRepositoryInstanceFactory(RepositoryFactory factory)
		{
			Value = factory;
		}

		/// <summary>
		/// Returns the RepositoryInstanceFactory object
		/// </summary>
		internal RepositoryFactory Factory
		{
			get { return Value; }
		}

		/// <summary>
		/// Return the repository based on the type
		/// </summary>
		/// <typeparam name="TRepository"></typeparam>
		/// <param name="unitOfWork"></param>
		/// <returns></returns>
		internal TRepository ResolveByType<TRepository>(IUnitOfWork unitOfWork)
			where TRepository : class, IRepository
		{
            var createMethod = this.Value.GetType().GetMethods(BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public)
				.First(x => x.Name == "Create" + typeof (TRepository).Name.Substring(1));

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