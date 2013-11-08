using System;
using System.Data;
using System.Web;
using Umbraco.Core;
using Umbraco.Core.Cache;
using umbraco.BusinessLogic;
using umbraco.DataLayer;
using System.Collections.Generic;
using umbraco.cms.businesslogic.web;
using Umbraco.Core.Models.Rdbms;

namespace umbraco.cms.businesslogic.relation
{
	/// <summary>
	/// Summary description for RelationType.
	/// </summary>
	public class RelationType
	{
		#region Declarations

		private int _id;
		private bool _dual;
		private string _name;
		//private Guid _parentObjectType;
		//private Guid _childObjectType;
		private string _alias;
        
		#endregion

        private static ISqlHelper SqlHelper
        {
            get { return Application.SqlHelper; }
        }

		#region Constructors

        /// <summary>
        /// Internal constructor to create a new relation type
        /// </summary>
        internal RelationType() { }

		public RelationType(int id)
		{
            var relationType = ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<RelationTypeDto>(
                             "SELECT id, [dual], name, alias FROM umbracoRelationType WHERE id = @id", new { id = id });
            if (relationType != null) PopulateFromDTO(relationType); 
            else throw new ArgumentException("No RelationType found for id " + id.ToString());
		}

		#endregion

		#region Properties

		public int Id
		{
			get { return _id; }
		}

		public string Name
		{
			get { return _name; }
			set
			{
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationTypeDto>("SET name = @0 WHERE id = @1", value, this.Id);  
			}
		}

		public string Alias
		{
			get { return _alias; }
			set
			{
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationTypeDto>("SET alias = @0 WHERE id = @1", value, this.Id);
			}
		}

		public bool Dual
		{
			get { return _dual; }
			set
			{
                ApplicationContext.Current.DatabaseContext.Database.Update<RelationTypeDto>("SET dual = @0 WHERE id = @1", value, this.Id);
			}
		}

		#endregion

        private void PopulateFromDTO(RelationTypeDto dto)
        {
            this._id = dto.Id;
            this._dual = dto.Dual;
            //this._parentObjectType = dto.ParentObjectType; // it was commented out in original method PopulateFromReader(...)
            //this._childObjectType = dto.ChildObjectType;  // it was commented out in original method PopulateFromReader(...)
            this._name = dto.Name;
            this._alias = dto.Alias; 
        }        

		#region Methods

		/// <summary>
		/// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
		/// </summary>
		public virtual void Save()
		{
		}

        /// <summary>
        /// Return all relation types
        /// </summary>
        /// <returns></returns>
        public static IEnumerable<RelationType> GetAll()
        {
            foreach (var relationTypeDto in ApplicationContext.Current.DatabaseContext.Database.Query<RelationTypeDto>(
                      "SELECT id, [dual], name, alias FROM umbracoRelationType"))
            {
                var relationType = (new RelationType());
                relationType.PopulateFromDTO(relationTypeDto);
                yield return relationType; 
            }
        }

		public static RelationType GetById(int id)
		{
			try
			{
			    return new RelationType(id);
			}
			catch
			{
				return null;
			}
		}

		public static RelationType GetByAlias(string Alias)
		{
			try
			{
                var  relationTypeDto =  ApplicationContext.Current.DatabaseContext.Database.FirstOrDefault<RelationTypeDto>(
                                 "SELECT id, [dual], name, alias FROM umbracoRelationType WHERE alias = @alias", new { alias = Alias });
                if (relationTypeDto == null) return null;
                var  relationType = (new RelationType());
                relationType.PopulateFromDTO(relationTypeDto);
                return relationType; 
			}
			catch
			{
				return null;
			}
		}
        
		#endregion

	}
}