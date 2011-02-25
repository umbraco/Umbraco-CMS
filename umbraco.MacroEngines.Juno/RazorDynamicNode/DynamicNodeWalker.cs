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
        public static DynamicNode Next(this DynamicNode context)
        {
            return context.Next(0);
        }
        public static DynamicNode Next(this DynamicNode context, int number)
        {
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.IndexOf(context);
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
        public static DynamicNode Previous(this DynamicNode context)
        {
            return context.Previous(0);
        }
        public static DynamicNode Previous(this DynamicNode context, int number)
        {
            if (context.ownerList != null)
            {
                List<DynamicNode> container = context.ownerList.Items.ToList();
                int currentIndex = container.IndexOf(context);
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
    }
}
