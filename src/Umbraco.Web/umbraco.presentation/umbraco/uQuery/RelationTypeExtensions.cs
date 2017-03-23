using System;
using System.Linq;
using umbraco.BusinessLogic;
using umbraco.cms.businesslogic.relation;

namespace umbraco
{
	/// <summary>
	/// uQuery extensions for the RelationType object.
	/// </summary>
	public static class RelationTypeExtensions
	{
		/// <summary>
		/// Extension method to return the Parent UmbracoObjectType
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <returns>an UmbracoObjectType value</returns>
		public static uQuery.UmbracoObjectType GetParentUmbracoObjectType(this RelationType relationType)
		{
		    using (var sqlHelper = Application.SqlHelper)
		    {
		        var guid = sqlHelper.ExecuteScalar<Guid>(
		            string.Concat("SELECT parentObjectType FROM umbracoRelationType WHERE id = ", relationType.Id));
		        return uQuery.GetUmbracoObjectType(guid);
		    }
		}

		/// <summary>
		/// Extension method to return the Child UmbracoObjectType
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <returns>an UmbracoObjectType value</returns>
		public static uQuery.UmbracoObjectType GetChildUmbracoObjectType(this RelationType relationType)
		{
		    using (var sqlHelper = Application.SqlHelper)
		    {
		        var guid = sqlHelper.ExecuteScalar<Guid>(
		            string.Concat("SELECT childObjectType FROM umbracoRelationType WHERE id = ", relationType.Id));
		        return uQuery.GetUmbracoObjectType(guid);
		    }
		}

		/// <summary>
		/// Creates a new relation for this relation type - also performs objectype validation
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="parentId">parentId of relation</param>
		/// <param name="childId">child Id of relation</param>
		public static void CreateRelation(this RelationType relationType, int parentId, int childId)
		{
			if ((uQuery.GetUmbracoObjectType(parentId) == relationType.GetParentUmbracoObjectType()) &&
			   (uQuery.GetUmbracoObjectType(childId) == relationType.GetChildUmbracoObjectType()))
			{
				Relation.MakeNew(parentId, childId, relationType, string.Empty);
			}
		}

		/// <summary>
		/// Determines whether the specified id exists as a relation for the current relation type
		/// </summary>
		/// <param name="relationType">Type of the relation.</param>
		/// <param name="id">The id.</param>
		/// <returns>
		///   <c>true</c> if the specified relation type has relations; otherwise, <c>false</c>.
		/// </returns>
		public static bool HasRelations(this RelationType relationType, int id)
		{
			if (Relation.GetRelations(id, relationType).Count() > 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		/// <summary>
		/// Wrapper for Relation.IsRelated(int, int, RelationType)
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="parentId">parentId to test</param>
		/// <param name="childId">childId to test</param>
		public static bool IsRelated(this RelationType relationType, int parentId, int childId)
		{
			return Relation.IsRelated(parentId, childId, relationType);
		}

		/// <summary>
		/// Extension method to get a relation from it's parent and child Ids
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="parentId">parentId of relation</param>
		/// <param name="childId">child Id of relation</param>
		/// <returns>null or the Relation matching supplied parentId and childId</returns>
		public static Relation GetRelation(this RelationType relationType, int parentId, int childId)
		{
			return Relation.GetRelations(parentId, relationType).Where(relation => relation.Child.Id == childId).First();
		}

		/// <summary>
		/// Gets the relations for the supplied id (wrapper for Relation.GetRelations)
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="id">the id</param>
		/// <returns>relations for the id</returns>
		public static Relation[] GetRelations(this RelationType relationType, int id)
		{
			return Relation.GetRelations(id, relationType);
		}

		/// <summary>
		/// Extension method to delete a relation, found by it's parent and child Ids
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="parentId">parent id of relation to delete</param>
		/// <param name="childId">child id of relation to delte</param>
		public static void DeleteRelation(this RelationType relationType, int parentId, int childId)
		{
			var relation = relationType.GetRelation(parentId, childId);
			if (relation != null)
			{
				relation.Delete();
			}
		}

		/// <summary>
		/// Wipes all relations associated with the id supplied in this relation type
		/// </summary>
		/// <param name="relationType">an instance of umbraco.cms.businesslogic.relation.RelationType</param>
		/// <param name="id">key id to wipe</param>
		public static void ClearRelations(this RelationType relationType, int id)
		{
			foreach (var relation in Relation.GetRelations(id, relationType))
			{
				relation.Delete();
			}
		}
	}
}