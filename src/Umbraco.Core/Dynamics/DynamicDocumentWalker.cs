using System;
using System.Collections.Generic;
using System.Linq;

namespace Umbraco.Core.Dynamics
{
	internal static class DynamicDocumentWalker
	{
		public static DynamicPublishedContent Up(this DynamicPublishedContent context)
		{
			return context.Up(0);
		}
		public static DynamicPublishedContent Up(this DynamicPublishedContent context, int number)
		{
			if (number == 0)
			{
				return context.Parent;
			}
			else
			{
				while ((context = context.Parent) != null && --number >= 0) ;
				return context;
			}
		}
		public static DynamicPublishedContent Up(this DynamicPublishedContent context, string nodeTypeAlias)
		{
			if (string.IsNullOrEmpty(nodeTypeAlias))
			{
				return context.Parent;
			}
			else
			{
				while ((context = context.Parent) != null && context.DocumentTypeAlias != nodeTypeAlias) ;
				return context;
			}
		}

		public static DynamicPublishedContent Down(this DynamicPublishedContent context)
		{
			return context.Down(0);
		}
		public static DynamicPublishedContent Down(this DynamicPublishedContent context, int number)
		{
			var children = new DynamicDocumentList(context.Children);
			if (number == 0)
			{
				return children.Items.First();
			}
			else
			{
				DynamicPublishedContent working = context;
				while (number-- >= 0)
				{
					working = children.Items.First();
					children = new DynamicDocumentList(working.Children);
				}
				return working;
			}
		}
		public static DynamicPublishedContent Down(this DynamicPublishedContent context, string nodeTypeAlias)
		{

			if (string.IsNullOrEmpty(nodeTypeAlias))
			{
				var children = new DynamicDocumentList(context.Children);
				return children.Items.First();
			}
			else
			{
				return context.Descendants(nodeTypeAlias).Items.FirstOrDefault();
			}
		}
		public static DynamicPublishedContent Next(this DynamicPublishedContent context)
		{
			return context.Next(0);
		}
		public static DynamicPublishedContent Next(this DynamicPublishedContent context, int number)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				var container = context.OwnerList.Items.ToList();
				var currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					return container.ElementAtOrDefault(currentIndex + (number + 1));
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
		public static DynamicPublishedContent Sibling(this DynamicPublishedContent context, int number)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				var container = context.OwnerList.Items.ToList();
				var currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					return container.ElementAtOrDefault(currentIndex + number);
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
		public static DynamicPublishedContent Sibling(this DynamicPublishedContent context, string nodeTypeAlias)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				var container = context.OwnerList.Items.ToList();
				var currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					var workingIndex = currentIndex + 1;
					while (workingIndex != currentIndex)
					{
						var working = container.ElementAtOrDefault(workingIndex);
						if (working != null && working.DocumentTypeAlias == nodeTypeAlias)
						{
							return working;
						}
						workingIndex++;
						if (workingIndex > container.Count)
						{
							workingIndex = 0;
						}
					}
					return null;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
		public static DynamicPublishedContent Next(this DynamicPublishedContent context, string nodeTypeAlias)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				var container = context.OwnerList.Items.ToList();
				var currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					var newIndex = container.FindIndex(currentIndex, n => n.DocumentTypeAlias == nodeTypeAlias);
					if (newIndex != -1)
					{
						return container.ElementAt(newIndex);
					}
					return null;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
		public static DynamicPublishedContent Previous(this DynamicPublishedContent context)
		{
			return context.Previous(0);
		}
		public static DynamicPublishedContent Previous(this DynamicPublishedContent context, int number)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				List<DynamicPublishedContent> container = context.OwnerList.Items.ToList();
				int currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					return container.ElementAtOrDefault(currentIndex + (number - 1));
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
		public static DynamicPublishedContent Previous(this DynamicPublishedContent context, string nodeTypeAlias)
		{
			if (context.OwnerList == null && context.Parent != null)
			{
				//var list = context.Parent.Children.Select(n => new DynamicNode(n));
				var list = context.Parent.Children;
				context.OwnerList = new DynamicDocumentList(list);
			}
			if (context.OwnerList != null)
			{
				List<DynamicPublishedContent> container = context.OwnerList.Items.ToList();
				int currentIndex = container.FindIndex(n => n.Id == context.Id);
				if (currentIndex != -1)
				{
					var previousNodes = container.Take(currentIndex).ToList();
					int newIndex = previousNodes.FindIndex(n => n.DocumentTypeAlias == nodeTypeAlias);
					if (newIndex != -1)
					{
						return container.ElementAt(newIndex);
					}
					return null;
				}
				else
				{
					throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicNodeList but could not retrieve the index for it's position in the list", context.Id));
				}
			}
			else
			{
				throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicNodeList", context.Id));
			}
		}
	}
}
