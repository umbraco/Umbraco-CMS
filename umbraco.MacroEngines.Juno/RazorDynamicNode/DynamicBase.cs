using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using umbraco.interfaces;

namespace umbraco.MacroEngines
{
    public class DynamicBase : DynamicObject
    {
        internal DynamicListBase ownerList;
        internal DynamicBackingItem backingItem;

        public int Level
        {
            get { if (backingItem == null) return 0; return backingItem.Level; }
        }

        public int Position()
        {
            return this.Index();
        }

        public int Id
        {
            get { if (backingItem == null) return 0; return backingItem.Id; }
        }

        public int Index()
        {
            if (this.ownerList == null && this.Parent != null)
            {
                var list = this.Parent.ChildrenAsList.ConvertAll(n => new DynamicBase(n));
                this.ownerList = new DynamicListBase(list);
            }
            if (this.ownerList != null)
            {
                List<DynamicBase> container = this.ownerList.Items.ToList();
                int currentIndex = container.FindIndex(n => backingItem.Id == this.Id);
                if (currentIndex != -1)
                {
                    return currentIndex;
                }
                else
                {
                    throw new IndexOutOfRangeException(string.Format("Node {0} belongs to a DynamicListBase but could not retrieve the index for it's position in the list", this.Id));
                }
            }
            else
            {
                throw new ArgumentNullException(string.Format("Node {0} has been orphaned and doesn't belong to a DynamicListBase", this.Id));
            }
        }
        public bool IsFirst()
        {
            return IsHelper(n => this.Index() == 0);
        }
        public string IsFirst(string valueIfTrue)
        {
            return IsHelper(n => this.Index() == 0, valueIfTrue);
        }
        public string IsFirst(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => this.Index() == 0, valueIfTrue, valueIfFalse);
        }
        public bool IsLast()
        {
            if (this.ownerList == null)
            {
                return false;
            }
            int count = this.ownerList.Items.Count;
            return IsHelper(n => this.Index() == count - 1);
        }
        public string IsLast(string valueIfTrue)
        {
            if (this.ownerList == null)
            {
                return string.Empty;
            }
            int count = this.ownerList.Items.Count;
            return IsHelper(n => this.Index() == count - 1, valueIfTrue);
        }
        public string IsLast(string valueIfTrue, string valueIfFalse)
        {
            if (this.ownerList == null)
            {
                return valueIfFalse;
            }
            int count = this.ownerList.Items.Count;
            return IsHelper(n => this.Index() == count - 1, valueIfTrue, valueIfFalse);
        }
        public bool IsEven()
        {
            return IsHelper(n => this.Index() % 2 == 0);
        }
        public string IsEven(string valueIfTrue)
        {
            return IsHelper(n => this.Index() % 2 == 0, valueIfTrue);
        }
        public string IsEven(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => this.Index() % 2 == 0, valueIfTrue, valueIfFalse);
        }
        public bool IsOdd()
        {
            return IsHelper(n => this.Index() % 2 == 1);
        }
        public string IsOdd(string valueIfTrue)
        {
            return IsHelper(n => this.Index() % 2 == 1, valueIfTrue);
        }
        public string IsOdd(string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => this.Index() % 2 == 1, valueIfTrue, valueIfFalse);
        }
        public bool IsEqual(DynamicBase other)
        {
            return IsHelper(n => backingItem.Id == other.Id);
        }
        public string IsEqual(DynamicBase other, string valueIfTrue)
        {
            return IsHelper(n => backingItem.Id == other.Id, valueIfTrue);
        }
        public string IsEqual(DynamicBase other, string valueIfTrue, string valueIfFalse)
        {
            return IsHelper(n => backingItem.Id == other.Id, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendant(DynamicBase other)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
        }
        public string IsDescendant(DynamicBase other, string valueIfTrue)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
        }
        public string IsDescendant(DynamicBase other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.Ancestors();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsDescendantOrSelf(DynamicBase other)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null);
        }
        public string IsDescendantOrSelf(DynamicBase other, string valueIfTrue)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue);
        }
        public string IsDescendantOrSelf(DynamicBase other, string valueIfTrue, string valueIfFalse)
        {
            var ancestors = this.AncestorsOrSelf();
            return IsHelper(n => ancestors.Items.Find(ancestor => ancestor.Id == other.Id) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestor(DynamicBase other)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
        }
        public string IsAncestor(DynamicBase other, string valueIfTrue)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
        }
        public string IsAncestor(DynamicBase other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.Descendants();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsAncestorOrSelf(DynamicBase other)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null);
        }
        public string IsAncestorOrSelf(DynamicBase other, string valueIfTrue)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue);
        }
        public string IsAncestorOrSelf(DynamicBase other, string valueIfTrue, string valueIfFalse)
        {
            var descendants = this.DescendantsOrSelf();
            return IsHelper(n => descendants.Items.Find(descendant => descendant.Id == other.Id) != null, valueIfTrue, valueIfFalse);
        }
        public bool IsHelper(Func<DynamicBase, bool> test)
        {
            return test(this);
        }
        public string IsHelper(Func<DynamicBase, bool> test, string valueIfTrue)
        {
            return IsHelper(test, valueIfTrue, string.Empty);
        }
        public string IsHelper(Func<DynamicBase, bool> test, string valueIfTrue, string valueIfFalse)
        {
            return test(this) ? valueIfTrue : valueIfFalse;
        }

        public DynamicBase AncestorOrSelf()
        {
            return AncestorOrSelf(node => node.Level == 1);
        }
        public DynamicBase AncestorOrSelf(int level)
        {
            return AncestorOrSelf(node => node.Level == level);
        }
        public DynamicBase AncestorOrSelf(string nodeTypeAlias)
        {
            return AncestorOrSelf(node => node.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicBase AncestorOrSelf(Func<DynamicBase, bool> func)
        {
            var node = this;
            while (node != null)
            {
                if (func(node)) return node;
                DynamicBase parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                    }
                    else
                    {
                        return node;
                    }
                }
                else
                {
                    return null;
                }
            }
            return node;
        }
        public DynamicListBase AncestorsOrSelf(Func<DynamicBase, bool> func)
        {
            List<DynamicBase> ancestorList = new List<DynamicBase>();
            var node = this;
            ancestorList.Add(node);
            while (node != null)
            {
                if (node.Level == 1) break;
                DynamicBase parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return new DynamicListBase(ancestorList);
        }
        public DynamicListBase AncestorsOrSelf()
        {
            return AncestorsOrSelf(n => true);
        }
        public DynamicListBase AncestorsOrSelf(string nodeTypeAlias)
        {
            return AncestorsOrSelf(n => backingItem.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicListBase AncestorsOrSelf(int level)
        {
            return AncestorsOrSelf(n => backingItem.Level <= level);
        }
        public DynamicListBase Descendants(string nodeTypeAlias)
        {
            return Descendants(p => p.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicListBase Descendants(int level)
        {
            return Descendants(p => p.Level >= level);
        }
        public DynamicListBase Descendants()
        {
            return Descendants(n => true);
        }
        public DynamicListBase Descendants(Func<INode, bool> func)
        {
            var flattenedNodes = this.backingItem.ChildrenAsList.Map(func, (INode n) => { return backingItem.ChildrenAsList; });
            return new DynamicListBase(flattenedNodes.ToList().ConvertAll(iNode => new DynamicBase(iNode)));
        }
        public DynamicListBase DescendantsOrSelf(int level)
        {
            return DescendantsOrSelf(p => p.Level >= level);
        }
        public DynamicListBase DescendantsOrSelf(string nodeTypeAlias)
        {
            return DescendantsOrSelf(p => p.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicListBase DescendantsOrSelf()
        {
            return DescendantsOrSelf(p => true);
        }
        public DynamicListBase DescendantsOrSelf(Func<INode, bool> func)
        {
            if (this.n != null)
            {
                var thisNode = new List<INode>();
                if (func(this.n))
                {
                    thisNode.Add(this.n);
                }
                var flattenedNodes = this.backingItem.ChildrenAsList.Map(func, (INode n) => { return backingItem.ChildrenAsList; });
                return new DynamicListBase(thisNode.Concat(flattenedNodes).ToList().ConvertAll(iNode => new DynamicBase(iNode)));
            }
            return new DynamicListBase(new List<INode>());
        }
        public DynamicListBase Ancestors(int level)
        {
            return Ancestors(n => backingItem.Level <= level);
        }
        public DynamicListBase Ancestors(string nodeTypeAlias)
        {
            return Ancestors(n => backingItem.NodeTypeAlias == nodeTypeAlias);
        }
        public DynamicListBase Ancestors()
        {
            return Ancestors(n => true);
        }
        public DynamicListBase Ancestors(Func<DynamicBase, bool> func)
        {
            List<DynamicBase> ancestorList = new List<DynamicBase>();
            var node = this;
            while (node != null)
            {
                if (node.Level == 1) break;
                DynamicBase parent = node.Parent;
                if (parent != null)
                {
                    if (this != parent)
                    {
                        node = parent;
                        if (func(node))
                        {
                            ancestorList.Add(node);
                        }
                    }
                    else
                    {
                        break;
                    }
                }
                else
                {
                    break;
                }
            }
            ancestorList.Reverse();
            return new DynamicListBase(ancestorList);
        }
        public DynamicBase Parent
        {
            get
            {
                if (n == null)
                {
                    return null;
                }
                if (backingItem.Parent != null)
                {
                    return new DynamicBase(backingItem.Parent);
                }
                if (n != null && backingItem.Id == 0)
                {
                    return this;
                }
                return null;
            }
        }
        
    }
}
