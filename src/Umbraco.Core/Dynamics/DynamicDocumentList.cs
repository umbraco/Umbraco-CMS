using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Umbraco.Core.Models;
using umbraco.interfaces;
using System.Collections;
using System.Reflection;

namespace Umbraco.Core.Dynamics
{
    public class DynamicDocumentList : DynamicObject, IEnumerable<DynamicDocument>
    {
		internal List<DynamicDocument> Items { get; set; }
        
        public DynamicDocumentList()
        {
            Items = new List<DynamicDocument>();
        }
        public DynamicDocumentList(IEnumerable<DynamicDocument> items)
        {
            List<DynamicDocument> list = items.ToList();
            list.ForEach(node => node.OwnerList = this);
            Items = list;
        }

		//public DynamicNodeList(IEnumerable<DynamicBackingItem> items)
		//{
		//    List<DynamicNode> list = items.ToList().ConvertAll(n => new DynamicNode(n));
		//    list.ForEach(node => node.OwnerList = this);
		//    Items = list;
		//}

        public DynamicDocumentList(IEnumerable<IDocument> items)
        {
            List<DynamicDocument> list = items.Select(x => new DynamicDocument(x)).ToList();
			list.ForEach(node => node.OwnerList = this);
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

			//TODO: We MUST cache the result here, it is very expensive to keep finding extension methods and processing this stuff!

            var name = binder.Name;
            if (name == "Where")
            {
                string predicate = args.First().ToString();
                var values = args.Skip(1).ToArray();
                result = new DynamicDocumentList(this.Where<DynamicDocument>(predicate, values).ToList());
                return true;
            }
            if (name == "OrderBy")
            {
                result = new DynamicDocumentList(this.OrderBy<DynamicDocument>(args.First().ToString()).ToList());
                return true;
            }
            if (name == "InGroupsOf")
            {
                int groupSize = 0;
                if (int.TryParse(args.First().ToString(), out groupSize))
                {
                    result = this.InGroupsOf<DynamicDocument>(groupSize);
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
                    result = this.GroupedInto<DynamicDocument>(groupCount);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupBy")
            {
                result = this.GroupBy<DynamicDocument>(args.First().ToString());
                return true;
            }
            if (name == "Average" || name == "Min" || name == "Max" || name == "Sum")
            {
                result = Aggregate(args, name);
                return true;
            }
            if (name == "Union")
            {
                if ((args.First() as IEnumerable<DynamicDocument>) != null)
                {
                    result = new DynamicDocumentList(this.Items.Union(args.First() as IEnumerable<DynamicDocument>));
                    return true;
                }
                if ((args.First() as DynamicDocumentList) != null)
                {
                    result = new DynamicDocumentList(this.Items.Union((args.First() as DynamicDocumentList).Items));
                    return true;
                }
            }
            if (name == "Except")
            {
                if ((args.First() as IEnumerable<DynamicDocument>) != null)
                {
                    result = new DynamicDocumentList(this.Items.Except(args.First() as IEnumerable<DynamicDocument>, new DynamicDocumentIdEqualityComparer()));
                    return true;
                }
                if ((args.First() as DynamicDocumentList) != null)
                {
                    result = new DynamicDocumentList(this.Items.Except((args.First() as DynamicDocumentList).Items, new DynamicDocumentIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Intersect")
            {
                if ((args.First() as IEnumerable<DynamicDocument>) != null)
                {
                    result = new DynamicDocumentList(this.Items.Intersect(args.First() as IEnumerable<DynamicDocument>, new DynamicDocumentIdEqualityComparer()));
                    return true;
                }
                if ((args.First() as DynamicDocumentList) != null)
                {
                    result = new DynamicDocumentList(this.Items.Intersect((args.First() as DynamicDocumentList).Items, new DynamicDocumentIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Distinct")
            {
                result = new DynamicDocumentList(this.Items.Distinct(new DynamicDocumentIdEqualityComparer()));
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
                        result = ExecuteExtensionMethod(args, name);
                        return true;
                    }
                    catch (TargetInvocationException)
                    {
                        //We do this to enable error checking of Razor Syntax when a method e.g. ElementAt(2) is used.
                        //When the Script is tested, there's no Children which means ElementAt(2) is invalid (IndexOutOfRange)
                        //Instead, we are going to return an empty DynamicNode.
                        result = new DynamicDocument();
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

        private object ExecuteExtensionMethod(object[] args, string name)
        {
            object result = null;

        	var methodTypesToFind = new[]
        		{
					typeof(IEnumerable<DynamicDocument>),
					typeof(DynamicDocumentList)
        		};

			//find known extension methods that match the first type in the list
        	MethodInfo toExecute = null;
			foreach(var t in methodTypesToFind)
			{
				toExecute = ExtensionMethodFinder.FindExtensionMethod(t, args, name, false);
				if (toExecute != null)
					break;
			}

			if (toExecute != null)
            {
				if (toExecute.GetParameters().First().ParameterType == typeof(DynamicDocumentList))
                {
                    var genericArgs = (new[] { this }).Concat(args);
					result = toExecute.Invoke(null, genericArgs.ToArray());
                }
                else
                {
                    var genericArgs = (new[] { Items }).Concat(args);
					result = toExecute.Invoke(null, genericArgs.ToArray());
                }
            }
            else
            {
                throw new MissingMethodException();
            }
            if (result != null)
            {
				if (result is IDocument)
				{
					result = new DynamicDocument((IDocument)result);
				}
				if (result is DynamicBackingItem)
				{
					result = new DynamicDocument((DynamicBackingItem)result);
				}
				if (result is IEnumerable<IDocument>)
				{
					result = new DynamicDocumentList((IEnumerable<IDocument>)result);
				}
				if (result is IEnumerable<DynamicDocument>)
				{
					result = new DynamicDocumentList((IEnumerable<DynamicDocument>)result);
				}		
            }
            return result;
        }

		//IEnumerator<DynamicNode> IEnumerable<DynamicNode>.GetEnumerator()
		//{
		//    return Items.GetEnumerator();
		//}

		//public IEnumerator GetEnumerator()
		//{
		//    return Items.GetEnumerator();
		//}

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
            int groupSize = (int)Math.Ceiling(((decimal)Items.Count() / groupCount));
            return new DynamicGrouping(
               this
               .Items
               .Select((node, index) => new KeyValuePair<int, DynamicDocument>(index, node))
               .GroupBy(kv => (object)(kv.Key / groupSize))
               .Select(item => new Grouping<object, DynamicDocument>()
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
                .Select((node, index) => new KeyValuePair<int, DynamicDocument>(index, node))
                .GroupBy(kv => (object)(kv.Key / groupSize))
                .Select(item => new Grouping<object, DynamicDocument>()
                {
                    Key = item.Key,
                    Elements = item.Select(inner => inner.Value)
                }));

        }

        public IQueryable Select(string predicate, params object[] values)
        {
            return DynamicQueryable.Select(Items.AsQueryable(), predicate, values);
        }

        public void Add(DynamicDocument document)
        {
            document.OwnerList = this;
            this.Items.Add(document);
        }
        public void Remove(DynamicDocument document)
        {
            if (this.Items.Contains(document))
            {
				document.OwnerList = null;
                this.Items.Remove(document);
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

    	public IEnumerator<DynamicDocument> GetEnumerator()
    	{
    		return Items.GetEnumerator();
    	}

    	IEnumerator IEnumerable.GetEnumerator()
    	{
    		return GetEnumerator();
    	}
    }
}
