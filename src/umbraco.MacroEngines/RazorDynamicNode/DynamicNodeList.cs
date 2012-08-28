using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Dynamic;
using Umbraco.Core;
using umbraco.interfaces;
using System.Collections;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Web.Compilation;
using System.Linq.Expressions;
using System.Linq.Dynamic;
namespace umbraco.MacroEngines
{
    public class DynamicNodeList : DynamicObject, IEnumerable<DynamicNode>
    {
        public List<DynamicNode> Items;
        public List<DynamicNode> get_Items()
        {
            return Items;
        }

        public DynamicNodeList()
        {
            Items = new List<DynamicNode>();
        }
        public DynamicNodeList(IEnumerable<DynamicNode> items)
        {
            List<DynamicNode> list = items.ToList();
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }
        public DynamicNodeList(IOrderedEnumerable<DynamicNode> items)
        {
            List<DynamicNode> list = items.ToList();
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }
        public DynamicNodeList(IEnumerable<DynamicBackingItem> items)
        {
            List<DynamicNode> list = items.ToList().ConvertAll(n => new DynamicNode(n));
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }

        public DynamicNodeList(IEnumerable<INode> items)
        {
            List<DynamicNode> list = items.Select(x => new DynamicNode(x)).ToList();
            list.ForEach(node => node.ownerList = this);
            Items = list;
        }
        public override bool TryGetIndex(GetIndexBinder binder, object[] indexes, out object result)
        {
            int index = (int)indexes[0];
            try
            {
                result = this.Items.ElementAt(index);
                return true;
            }
            catch (IndexOutOfRangeException)
            {
                result = new DynamicNull();
                return true;
            }
        }
        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name;
            if (name == "Where")
            {
                string predicate = args.First().ToString();
                var values = args.Skip(1).ToArray();
                result = new DynamicNodeList(this.Where<DynamicNode>(predicate, values).ToList());
                return true;
            }
            if (name == "OrderBy")
            {
                result = new DynamicNodeList(this.OrderBy<DynamicNode>(args.First().ToString()).ToList());
                return true;
            }
			if (name == "Take")
			{
				result = new DynamicNodeList(this.Take((int)args.First()));
				return true;
			}
            if (name == "InGroupsOf")
            {
                int groupSize = 0;
                if (int.TryParse(args.First().ToString(), out groupSize))
                {
                    result = this.InGroupsOf<DynamicNode>(groupSize);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupedInto")
            {
                int groupCount = 0;
                if (int.TryParse(args.First().ToString(), out groupCount))
                {
                    result = this.GroupedInto<DynamicNode>(groupCount);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupBy")
            {
                result = this.GroupBy<DynamicNode>(args.First().ToString());
                return true;
            }
            if (name == "Average" || name == "Min" || name == "Max" || name == "Sum")
            {
                result = Aggregate(args, name);
                return true;
            }
            if (name == "Union")
            {
                if ((args.First() as IEnumerable<DynamicNode>) != null)
                {
                    result = new DynamicNodeList(this.Items.Union(args.First() as IEnumerable<DynamicNode>));
                    return true;
                }
                if ((args.First() as DynamicNodeList) != null)
                {
                    result = new DynamicNodeList(this.Items.Union((args.First() as DynamicNodeList).Items));
                    return true;
                }
            }
            if (name == "Except")
            {
                if ((args.First() as IEnumerable<DynamicNode>) != null)
                {
                    result = new DynamicNodeList(this.Items.Except(args.First() as IEnumerable<DynamicNode>, new DynamicNodeIdEqualityComparer()));
                    return true;
                }
                if ((args.First() as DynamicNodeList) != null)
                {
                    result = new DynamicNodeList(this.Items.Except((args.First() as DynamicNodeList).Items, new DynamicNodeIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Intersect")
            {
                if ((args.First() as IEnumerable<DynamicNode>) != null)
                {
                    result = new DynamicNodeList(this.Items.Intersect(args.First() as IEnumerable<DynamicNode>, new DynamicNodeIdEqualityComparer()));
                    return true;
                }
                if ((args.First() as DynamicNodeList) != null)
                {
                    result = new DynamicNodeList(this.Items.Intersect((args.First() as DynamicNodeList).Items, new DynamicNodeIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Distinct")
            {
                result = new DynamicNodeList(this.Items.Distinct(new DynamicNodeIdEqualityComparer()));
                return true;
            }
            if (name == "Pluck" || name == "Select")
            {
                result = Pluck(args);
                return true;
            }
            try
            {
                //Property?
                result = Items.GetType().InvokeMember(binder.Name,
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.GetProperty,
                                                  null,
                                                  Items,
                                                  args);
                return true;
            }
            catch (MissingMethodException)
            {
                try
                {
                    //Static or Instance Method?
                    result = Items.GetType().InvokeMember(binder.Name,
                                                  System.Reflection.BindingFlags.Instance |
                                                  System.Reflection.BindingFlags.Public |
                                                  System.Reflection.BindingFlags.Static |
                                                  System.Reflection.BindingFlags.InvokeMethod,
                                                  null,
                                                  Items,
                                                  args);
                    return true;
                }
                catch (MissingMethodException)
                {

                    try
                    {
                        result = ExecuteExtensionMethod(args, name, false);
                        return true;
                    }
                    catch (TargetInvocationException)
                    {
                        //We do this to enable error checking of Razor Syntax when a method e.g. ElementAt(2) is used.
                        //When the Script is tested, there's no Children which means ElementAt(2) is invalid (IndexOutOfRange)
                        //Instead, we are going to return an empty DynamicNode.
                        result = new DynamicNode();
                        return true;
                    }

                    catch
                    {
                        result = null;
                        return false;
                    }

                }


            }
            catch
            {
                result = null;
                return false;
            }

        }
        private T Aggregate<T>(List<T> data, string name) where T : struct
        {
            switch (name)
            {
                case "Min":
                    return data.Min<T>();
                case "Max":
                    return data.Max<T>();
                case "Average":
                    if (typeof(T) == typeof(int))
                    {
                        return (T)Convert.ChangeType((data as List<int>).Average(), typeof(T));
                    }
                    if (typeof(T) == typeof(decimal))
                    {
                        return (T)Convert.ChangeType((data as List<decimal>).Average(), typeof(T));
                    }
                    break;
                case "Sum":
                    if (typeof(T) == typeof(int))
                    {
                        return (T)Convert.ChangeType((data as List<int>).Sum(), typeof(T));
                    }
                    if (typeof(T) == typeof(decimal))
                    {
                        return (T)Convert.ChangeType((data as List<decimal>).Sum(), typeof(T));
                    }
                    break;
            }
            return default(T);
        }
        private object Aggregate(object[] args, string name)
        {
            object result;
            string predicate = args.First().ToString();
            var values = args.Skip(1).ToArray();
            var query = (IQueryable<object>)this.Select(predicate, values);
            object firstItem = query.FirstOrDefault();
            if (firstItem == null)
            {
                result = new DynamicNull();
            }
            else
            {
                var types = from i in query
                            group i by i.GetType() into g
                            where g.Key != typeof(DynamicNull)
                            orderby g.Count() descending
                            select new { g, Instances = g.Count() };
                var dominantType = types.First().g.Key;
                //remove items that are not the dominant type
                //e.g. string,string,string,string,false[DynamicNull],string
                var itemsOfDominantTypeOnly = query.ToList();
                itemsOfDominantTypeOnly.RemoveAll(item => !item.GetType().IsAssignableFrom(dominantType));
                if (dominantType == typeof(string))
                {
                    throw new ArgumentException("Can only use aggregate methods on properties which are numeric");
                }
                else if (dominantType == typeof(int))
                {
                    List<int> data = (List<int>)itemsOfDominantTypeOnly.Cast<int>().ToList();
                    return Aggregate<int>(data, name);
                }
                else if (dominantType == typeof(decimal))
                {
                    List<decimal> data = (List<decimal>)itemsOfDominantTypeOnly.Cast<decimal>().ToList();
                    return Aggregate<decimal>(data, name);
                }
                else if (dominantType == typeof(bool))
                {
                    throw new ArgumentException("Can only use aggregate methods on properties which are numeric or datetime");
                }
                else if (dominantType == typeof(DateTime))
                {
                    if (name != "Min" || name != "Max")
                    {
                        throw new ArgumentException("Can only use aggregate min or max methods on properties which are datetime");
                    }
                    List<DateTime> data = (List<DateTime>)itemsOfDominantTypeOnly.Cast<DateTime>().ToList();
                    return Aggregate<DateTime>(data, name);
                }
                else
                {
                    result = query.ToList();
                }
            }
            return result;
        }
        private object Pluck(object[] args)
        {
            object result;
            string predicate = args.First().ToString();
            var values = args.Skip(1).ToArray();
            var query = (IQueryable<object>)this.Select(predicate, values);
            object firstItem = query.FirstOrDefault();
            if (firstItem == null)
            {
                result = new List<object>();
            }
            else
            {
                var types = from i in query
                            group i by i.GetType() into g
                            where g.Key != typeof(DynamicNull)
                            orderby g.Count() descending
                            select new { g, Instances = g.Count() };
                var dominantType = types.First().g.Key;
                //remove items that are not the dominant type
                //e.g. string,string,string,string,false[DynamicNull],string
                var itemsOfDominantTypeOnly = query.ToList();
                itemsOfDominantTypeOnly.RemoveAll(item => !item.GetType().IsAssignableFrom(dominantType));
                if (dominantType == typeof(string))
                {
                    result = (List<string>)itemsOfDominantTypeOnly.Cast<string>().ToList();
                }
                else if (dominantType == typeof(int))
                {
                    result = (List<int>)itemsOfDominantTypeOnly.Cast<int>().ToList();
                }
                else if (dominantType == typeof(decimal))
                {
                    result = (List<decimal>)itemsOfDominantTypeOnly.Cast<decimal>().ToList();
                }
                else if (dominantType == typeof(bool))
                {
                    result = (List<bool>)itemsOfDominantTypeOnly.Cast<bool>().ToList();
                }
                else if (dominantType == typeof(DateTime))
                {
                    result = (List<DateTime>)itemsOfDominantTypeOnly.Cast<DateTime>().ToList();
                }
                else
                {
                    result = query.ToList();
                }
            }
            return result;
        }

        private object ExecuteExtensionMethod(object[] args, string name, bool argsContainsThis)
        {
            object result = null;

            MethodInfo methodToExecute = ExtensionMethodFinder.FindExtensionMethod(typeof(IEnumerable<DynamicNode>), args, name, false);
            if (methodToExecute == null)
            {
                methodToExecute = ExtensionMethodFinder.FindExtensionMethod(typeof(DynamicNodeList), args, name, false);
            }
            if (methodToExecute != null)
            {
                if (methodToExecute.GetParameters().First().ParameterType == typeof(DynamicNodeList))
                {
                    var genericArgs = (new[] { this }).Concat(args);
                    result = methodToExecute.Invoke(null, genericArgs.ToArray());
                }
				else if (TypeHelper.IsTypeAssignableFrom<IQueryable>(methodToExecute.GetParameters().First().ParameterType))
				{
					//if it is IQueryable, we'll need to cast Items AsQueryable
					var genericArgs = (new[] { Items.AsQueryable() }).Concat(args);
					result = methodToExecute.Invoke(null, genericArgs.ToArray());
				}
                else
                {
                    var genericArgs = (new[] { Items }).Concat(args);
                    result = methodToExecute.Invoke(null, genericArgs.ToArray());
                }
            }
            else
            {
                throw new MissingMethodException();
            }
            if (result != null)
            {
                if (result is IEnumerable<INode>)
                {
                    result = new DynamicNodeList((IEnumerable<INode>)result);
                }
                if (result is IEnumerable<DynamicNode>)
                {
                    result = new DynamicNodeList((IEnumerable<DynamicNode>)result);
                }
                if (result is INode)
                {
                    result = new DynamicNode((INode)result);
                }
            }
            return result;
        }

		public IEnumerator<DynamicNode> GetEnumerator()
		{
			return Items.GetEnumerator();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return GetEnumerator();
		}

        public IQueryable<T> Where<T>(string predicate, params object[] values)
        {
            return ((IQueryable<T>)Items.AsQueryable()).Where(predicate, values);
        }
        public IQueryable<T> OrderBy<T>(string key)
        {
            return ((IQueryable<T>)Items.AsQueryable()).OrderBy(key);
        }
        public DynamicGrouping GroupBy<T>(string key)
        {
            DynamicGrouping group = new DynamicGrouping(this, key);
            return group;
        }
        public DynamicGrouping GroupedInto<T>(int groupCount)
        {
            int groupSize = (int)Math.Ceiling(((decimal)Items.Count / groupCount));
            return new DynamicGrouping(
               this
               .Items
               .Select((node, index) => new KeyValuePair<int, DynamicNode>(index, node))
               .GroupBy(kv => (object)(kv.Key / groupSize))
               .Select(item => new Grouping<object, DynamicNode>()
               {
                   Key = item.Key,
                   Elements = item.Select(inner => inner.Value)
               }));
        }
        public DynamicGrouping InGroupsOf<T>(int groupSize)
        {
            return new DynamicGrouping(
                this
                .Items
                .Select((node, index) => new KeyValuePair<int, DynamicNode>(index, node))
                .GroupBy(kv => (object)(kv.Key / groupSize))
                .Select(item => new Grouping<object, DynamicNode>()
                {
                    Key = item.Key,
                    Elements = item.Select(inner => inner.Value)
                }));

        }

        public IQueryable Select(string predicate, params object[] values)
        {
            return DynamicQueryable.Select(Items.AsQueryable(), predicate, values);
        }

        public void Add(DynamicNode node)
        {
            node.ownerList = this;
            this.Items.Add(node);
        }
        public void Remove(DynamicNode node)
        {
            if (this.Items.Contains(node))
            {
                node.ownerList = null;
                this.Items.Remove(node);
            }
        }
        public bool IsNull()
        {
            return false;
        }
        public bool HasValue()
        {
            return true;
        }
    }
}
