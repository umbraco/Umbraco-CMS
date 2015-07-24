using System;
using System.Data;
using System.Linq;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.BusinessLogic;
using Umbraco.Core.Models;
using umbraco.DataLayer;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for RelationType.
	/// </summary>
    [Obsolete("Use the IRelationService instead")]
	public class RelationType
	{
		#region Declarations

	    internal IRelationType RelationTypeEntity;

		#endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		#region Constructors

	    /// <summary>
	    /// Internal constructor to create a new relation type
	    /// </summary>
	    internal RelationType(IRelationType rt)
	    {
	        RelationTypeEntity = rt;
	    }

		public RelationType(int id)
		{
		    RelationTypeEntity = ApplicationContext.Current.Services.RelationService.GetRelationTypeById(id);
		    if (RelationTypeEntity == null) throw new NullReferenceException("No relation type found with id " + id);
		}

		#endregion

		#region Properties

		public int Id
		{
            get { return RelationTypeEntity.Id; }
		}

		public string Name
		{
            get { return RelationTypeEntity.Name; }
			set { RelationTypeEntity.Name = value; }
		}

		public string Alias
		{
            get { return RelationTypeEntity.Alias; }
			set { RelationTypeEntity.Alias = value; }
		}

		public bool Dual
		{
            get { return RelationTypeEntity.IsBidirectional; }
			set { RelationTypeEntity.IsBidirectional = value; }
		}

		#endregion

		#region Methods

		/// <summary>
		/// Used to persist object changes to the databasey
		/// </summary>
		public virtual void Save()
		{
		    ApplicationContext.Current.Services.RelationService.Save(RelationTypeEntity);
		}

        /// <summary>
        /// Return all relation types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RelationType> GetAll()
        {
            return ApplicationContext.Current.Services.RelationService.GetAllRelationTypes()
                .Select(x => new RelationType(x));
        }

		public static RelationType GetById(int id)
		{
		    var found = ApplicationContext.Current.Services.RelationService.GetRelationTypeById(id);
		    return found == null ? null : new RelationType(found);
		}

		public static RelationType GetByAlias(string Alias)
		{
            var found = ApplicationContext.Current.Services.RelationService.GetRelationTypeByAlias(Alias);
            return found == null ? null : new RelationType(found);
		}
        
		#endregion

	}
}