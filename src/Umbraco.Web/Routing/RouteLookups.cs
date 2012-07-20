using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Umbraco.Core;

namespace Umbraco.Web.Routing
{
	/// <summary>
	/// Represents a collection of ILookup used for routing that are registered in the application
	/// </summary>
	internal class RouteLookups
	{
		private static readonly List<ILookup> Lookups = new List<ILookup>();
		private static readonly ReaderWriterLockSlim Lock = new ReaderWriterLockSlim();

		/// <summary>
		/// Singleton accessor
		/// </summary>
		public static RouteLookups Current { get; internal set; }

		internal RouteLookups(IEnumerable<ILookup> lookups)
		{
			Lookups.AddRange(SortByWeight(lookups));
		}

		/// <summary>
		/// Returns all of the lookups
		/// </summary>
		/// <returns></returns>
		public IEnumerable<ILookup> GetLookups()
		{
			return Lookups;
		} 

		/// <summary>
		/// Removes an ILookup based on the specified Type
		/// </summary>
		/// <typeparam name="T"></typeparam>
		public void RemoveLookup<T>()
			where T : ILookup
		{
			using (new WriteLock(Lock))
			{
				Lookups.Remove(Lookups.SingleOrDefault(x => x is T));	
			}			
		}

	
		/// <summary>
		/// Adds a new lookup to the end of the list
		/// </summary>
		/// <param name="lookup"></param>
		public void AddLookup(ILookup lookup)
		{
			if (CheckExists(lookup)) 
				throw new InvalidOperationException("The lookup type " + lookup + " already exists in the lookup collection");

			using (new WriteLock(Lock))
			{
				Lookups.Add(lookup);
			}			
		}

		/// <summary>
		/// Inserts a lookup at the specified index
		/// </summary>
		/// <param name="index"></param>
		/// <param name="lookup"></param>
		public void InsertLookup(int index, ILookup lookup)
		{
			if (CheckExists(lookup))
				throw new InvalidOperationException("The lookup type " + lookup + " already exists in the lookup collection");
			
			using (new WriteLock(Lock))
			{
				Lookups.Insert(index, lookup);
			}			
		}

		/// <summary>
		/// checks if a lookup already exists by type
		/// </summary>
		/// <param name="lookup"></param>
		/// <returns></returns>
		private static bool CheckExists(ILookup lookup)
		{
			return Lookups.Any(x => x.GetType() == lookup.GetType());
		}

		/// <summary>
		/// Sorts the ILookups in the list based on an attribute weight if one is specified
		/// </summary>
		/// <param name="lookups"></param>
		/// <returns></returns>
		private static IEnumerable<ILookup> SortByWeight(IEnumerable<ILookup> lookups)
		{
			return lookups.OrderBy(x =>
				{
					var attribute = x.GetType().GetCustomAttributes(true).OfType<LookupWeightAttribute>().SingleOrDefault();
					return attribute == null ? LookupWeightAttribute.DefaultWeight : attribute.Weight;
				}).ToList();
		}

	}
}