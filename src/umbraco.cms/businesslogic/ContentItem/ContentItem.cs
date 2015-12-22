using System;

namespace umbraco.cms.businesslogic.contentitem
{
	/// <summary>
	/// Summary description for ContentItem.
	/// </summary>
	public class ContentItem : Content
	{
	    public ContentItem(int id) : base(id)
		{
		}
		public ContentItem(Guid id) : base(id)
		{
		}

		public static void DeleteFromType(ContentItemType cit) 
		{
            var objs = Content.getContentOfContentType(cit);
			foreach (Content c in objs) 
			{
				// due to recursive structure document might already been deleted..
				if (CMSNode.IsNode(c.UniqueId)) 
				{
					ContentItem tmp = new ContentItem(c.UniqueId);
					tmp.delete();
				}
			}
		}

		

		
		new public void delete() 
		{
			foreach (ContentItem d in this.Children) 
			{
				d.delete();
			}
			base.delete();
		}
		
		new public ContentItem[] Children 
		{
			get
			{
				BusinessLogic.console.IconI[] tmp = base.Children;
				ContentItem[] retval = new ContentItem[tmp.Length];
				for (int i = 0; i < tmp.Length; i++) retval[i] = new ContentItem(tmp[i].UniqueId);
				return retval;
			}
		}
	}
}
