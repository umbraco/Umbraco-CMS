using System;

namespace umbraco.cms.presentation.developer.RelationTypes
{
	/// <summary>
	///  This is used to build a collection of relations from a single sql statement,
	///  as the umbraco.cms.businesslogic.relation.Relation obj will hit the DB for each instace it creates
	/// </summary>
	internal struct ReadOnlyRelation
	{
		/// <summary>
		/// Gets or sets the Relation Id
		/// </summary>
		public int Id { get; set; }

		/// <summary>
		/// Gets or sets Relation Parent Id
		/// </summary>
		public int ParentId { get; set; }

		/// <summary>
		/// Gets or sets Relation Parent Text
		/// </summary>
		public string ParentText { get; set; }

		/// <summary>
		/// Gets or sets Relation Child Id
		/// </summary>
		public int ChildId { get; set; }

		/// <summary>
		/// Gets or sets Relation Child Text
		/// </summary>
		public string ChildText { get; set; }

		/// <summary>
		/// Gets or sets Relation RelationType Id
		/// </summary>
		public int RelType { get; set; }

		/// <summary>
		/// Gets or sets Relation DateTime
		/// </summary>
		public DateTime DateTime { get; set; }

		/// <summary>
		/// Gets or sets Relation Comment
		/// </summary>
		public string Comment { get; set; }
	}
}