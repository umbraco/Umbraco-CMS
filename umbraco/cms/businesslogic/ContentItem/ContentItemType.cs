using System;

namespace umbraco.cms.businesslogic.contentitem
{
	/// <summary>
	/// Summary description for ContentItemType.
	/// </summary>
	public class ContentItemType : ContentType
	{
		private static Guid _objectType = new Guid("7a333c54-6f43-40a4-86a2-18688dc7e532 ");
		public ContentItemType(int id) : base(id)
		{
			//
			// TODO: Add constructor logic here
			//
		}
		public ContentItemType(Guid id) : base(id)
		{
			//
			// TODO: Add constructor logic here
			//
		}


        /// <summary>
        /// Used to persist object changes to the database. In Version3.0 it's just a stub for future compatibility
        /// </summary>
        public override void Save()
        {
        }




		public static ContentItemType MakeNew( BusinessLogic.User u,string Text) 
		{
		
			int ParentId= -1;
			int level = 1;
			Guid uniqueId = Guid.NewGuid();
			CMSNode n = CMSNode.MakeNew(ParentId, _objectType, u.Id, level,Text, uniqueId);

			ContentType.Create(n.Id, Text,"");

			return new ContentItemType(n.Id);
		}

		new public static ContentItemType[] GetAll 
		{
			get
			{
				Guid[] Ids = CMSNode.getAllUniquesFromObjectType(_objectType);
				ContentItemType[] retVal = new ContentItemType[Ids.Length];
				for (int i = 0; i  < Ids.Length; i++) retVal[i] = new ContentItemType(Ids[i]);
				return retVal;
			}
		}
		
		new public void delete() 
		{
			// delete all documents of this type
			ContentItem.DeleteFromType(this);
			// Delete contentType
			base.delete();
		}
	}
}
