using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public static class DynamicNodeWalker
    {
        public static DynamicNode Up(this DynamicNode context)
        {
            return context.Up(0);
        }
        public static DynamicNode Up(this DynamicNode context, int number)
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
        public static DynamicNode Up(this DynamicNode context, string nodeTypeAlias)
        {
            if (string.IsNullOrEmpty(nodeTypeAlias))
            {
                return context.Parent;
            }
            else
            {
                while ((context = context.Parent) != null && context.NodeTypeAlias != nodeTypeAlias) ;
                return context;
            }
        }

        public static DynamicNode Down(this DynamicNode context)
        {
            return context.Down(0);
        }
        public static DynamicNode Down(this DynamicNode context, int number)
        {
            DynamicNodeList children = new DynamicNodeList(context.ChildrenAsList);
            if (number == 0)
            {
                return children.Items.First();
            }
            else
            {
                DynamicNode working = context;
                while (number-- >= 0)
                {
                    working = children.Items.First();
                    children = new DynamicNodeList(working.ChildrenAsList);
                }
                return working;
            }
        }
        public static DynamicNode Down(this DynamicNode context, string nodeTypeAlias)
        {

            if (string.IsNullOrEmpty(nodeTypeAlias))
            {
                DynamicNodeList children = new DynamicNodeList(context.ChildrenAsList);
                return children.Items.First();
            }
            else
            {
                return context.Descendants(nodeTypeAlias).Items.FirstOrDefault();
            }
        }
        public static DynamicNode Next(this DynamicNode context)
        {
            return context.Next(0);
        }
        public static DynamicNode Next(this DynamicNode context, int number)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                //var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                //context.ownerList = new DynamicNodeList(list);
            	context.ownerList = context.Parent.ChildrenAsList;
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => n.Id == context.Id);
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
        public static DynamicNode Sibling(this DynamicNode context, int number)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                context.ownerList = new DynamicNodeList(list);
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => n.Id == context.Id);
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
        public static DynamicNode Sibling(this DynamicNode context, string nodeTypeAlias)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                context.ownerList = new DynamicNodeList(list);
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => n.Id == context.Id);
                if (currentIndex != -1)
                {
                    int workingIndex = currentIndex + 1;
                    while (workingIndex != currentIndex)
                    {
                        var working = container.ElementAtOrDefault(workingIndex);
                        if (working != null && working.NodeTypeAlias == nodeTypeAlias)
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
        public static DynamicNode Next(this DynamicNode context, string nodeTypeAlias)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                context.ownerList = new DynamicNodeList(list);
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => n.Id == context.Id);
                if (currentIndex != -1)
                {
                    int newIndex = container.FindIndex(currentIndex, n => n.NodeTypeAlias == nodeTypeAlias);
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
        public static DynamicNode Previous(this DynamicNode context)
        {
            return context.Previous(0);
        }
        public static DynamicNode Previous(this DynamicNode context, int number)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                //var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                //context.ownerList = new DynamicNodeList(list);
            	context.ownerList = context.Parent.ChildrenAsList;
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
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
        public static DynamicNode Previous(this DynamicNode context, string nodeTypeAlias)
        {
            if (context.ownerList == null && context.Parent != null)
            {
                var list = context.Parent.ChildrenAsList.Select(n => new DynamicNode(n));
                context.ownerList = new DynamicNodeList(list);
            }
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => n.Id == context.Id);
                if (currentIndex != -1)
                {
                    var previousNodes = container.Take(currentIndex).ToList();
                    int newIndex = previousNodes.FindIndex(n => n.NodeTypeAlias == nodeTypeAlias);
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
