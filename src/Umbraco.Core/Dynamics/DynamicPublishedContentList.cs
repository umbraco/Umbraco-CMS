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
    public class DynamicPublishedContentList : DynamicObject, IEnumerable<DynamicPublishedContent>
    {
		internal List<DynamicPublishedContent> Items { get; set; }
        
        public DynamicPublishedContentList()
        {
            Items = new List<DynamicPublishedContent>();
        }
        public DynamicPublishedContentList(IEnumerable<DynamicPublishedContent> items)
        {
            List<DynamicPublishedContent> list = items.ToList();
            list.ForEach(node => node.OwnerList = this);
            Items = list;
        }

        public DynamicPublishedContentList(IEnumerable<IPublishedContent> items)
        {
            List<DynamicPublishedContent> list = items.Select(x => new DynamicPublishedContent(x)).ToList();
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

			//TODO: Nowhere here are we checking if args is the correct length!

			//NOTE: For many of these we could actually leave them out since we are executing custom extension methods and because
			// we implement IEnumerable<T> they will execute just fine, however, to do that will be quite a bit slower than checking here.

            var name = binder.Name;
            if (name == "Where")
            {
                string predicate = args.First().ToString();
                var values = args.Skip(1).ToArray();
				//TODO: We are pre-resolving the where into a ToList() here which will have performance impacts if there where clauses
				// are nested! We should somehow support an QueryableDocumentList!
				result = new DynamicPublishedContentList(this.Where<DynamicPublishedContent>(predicate, values).ToList());				
                return true;
            }
            if (name == "OrderBy")
            {
				//TODO: We are pre-resolving the where into a ToList() here which will have performance impacts if there where clauses
				// are nested! We should somehow support an QueryableDocumentList!
				result = new DynamicPublishedContentList(this.OrderBy<DynamicPublishedContent>(args.First().ToString()).ToList());
                return true;
            }
			if (name == "Take")
			{
				result = new DynamicPublishedContentList(this.Take((int)args.First()));
				return true;
			}
			if (name == "Skip")
			{
				result = new DynamicPublishedContentList(this.Skip((int)args.First()));
				return true;
			}
        	if (name == "InGroupsOf")
            {
                int groupSize = 0;
                if (int.TryParse(args.First().ToString(), out groupSize))
                {
                    result = this.InGroupsOf<DynamicPublishedContent>(groupSize);
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
                    result = this.GroupedInto<DynamicPublishedContent>(groupCount);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupBy")
            {
                result = this.GroupBy<DynamicPublishedContent>(args.First().ToString());
                return true;
            }
            if (name == "Average" || name == "Min" || name == "Max" || name == "Sum")
            {
                result = Aggregate(args, name);
                return true;
            }
            if (name == "Union")
            {
                if ((args.First() as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Union(args.First() as IEnumerable<DynamicPublishedContent>));
                    return true;
                }
                if ((args.First() as DynamicPublishedContentList) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Union((args.First() as DynamicPublishedContentList).Items));				
                    return true;
                }
            }
            if (name == "Except")
            {
                if ((args.First() as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Except(args.First() as IEnumerable<DynamicPublishedContent>, new DynamicPublishedContentIdEqualityComparer()));					
                    return true;
                }
                if ((args.First() as DynamicPublishedContentList) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Except((args.First() as DynamicPublishedContentList).Items, new DynamicPublishedContentIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Intersect")
            {
                if ((args.First() as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Intersect(args.First() as IEnumerable<DynamicPublishedContent>, new DynamicPublishedContentIdEqualityComparer()));					
                    return true;
                }
                if ((args.First() as DynamicPublishedContentList) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Intersect((args.First() as DynamicPublishedContentList).Items, new DynamicPublishedContentIdEqualityComparer()));
                    return true;
                }
            }
            if (name == "Distinct")
            {
                result = new DynamicPublishedContentList(this.Items.Distinct(new DynamicPublishedContentIdEqualityComparer()));
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
                    	result = DynamicPublishedContent.Empty();
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
					typeof(IEnumerable<DynamicPublishedContent>),
					typeof(DynamicPublishedContentList)
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
				if (toExecute.GetParameters().First().ParameterType == typeof(DynamicPublishedContentList))
                {
                    var genericArgs = (new[] { this }).Concat(args);
					result = toExecute.Invoke(null, genericArgs.ToArray());
                }
				else if (TypeHelper.IsTypeAssignableFrom<IQueryable>(toExecute.GetParameters().First().ParameterType))
				{
					//if it is IQueryable, we'll need to cast Items AsQueryable
					var genericArgs = (new[] { Items.AsQueryable() }).Concat(args);
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
				if (result is IPublishedContent)
				{
					result = new DynamicPublishedContent((IPublishedContent)result);
				}
				if (result is IEnumerable<IPublishedContent>)
				{
					result = new DynamicPublishedContentList((IEnumerable<IPublishedContent>)result);
				}
				if (result is IEnumerable<DynamicPublishedContent>)
				{
					result = new DynamicPublishedContentList((IEnumerable<DynamicPublishedContent>)result);
				}		
            }
            return result;
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
            var group = new DynamicGrouping(this, key);
            return group;
        }
        public DynamicGrouping GroupedInto<T>(int groupCount)
        {
            int groupSize = (int)Math.Ceiling(((decimal)Items.Count() / groupCount));
            return new DynamicGrouping(
               this
               .Items
               .Select((node, index) => new KeyValuePair<int, DynamicPublishedContent>(index, node))
               .GroupBy(kv => (object)(kv.Key / groupSize))
               .Select(item => new Grouping<object, DynamicPublishedContent>()
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
                .Select((node, index) => new KeyValuePair<int, DynamicPublishedContent>(index, node))
                .GroupBy(kv => (object)(kv.Key / groupSize))
                .Select(item => new Grouping<object, DynamicPublishedContent>()
                {
                    Key = item.Key,
                    Elements = item.Select(inner => inner.Value)
                }));

        }

        public IQueryable Select(string predicate, params object[] values)
        {
            return Items.AsQueryable().Select(predicate, values);
        }

        public void Add(DynamicPublishedContent publishedContent)
        {
            publishedContent.OwnerList = this;
            this.Items.Add(publishedContent);
        }
        public void Remove(DynamicPublishedContent publishedContent)
        {
            if (this.Items.Contains(publishedContent))
            {
				publishedContent.OwnerList = null;
                this.Items.Remove(publishedContent);
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

    	public IEnumerator<DynamicPublishedContent> GetEnumerator()
    	{
    		return Items.GetEnumerator();
    	}

    	IEnumerator IEnumerable.GetEnumerator()
    	{
    		return GetEnumerator();
    	}
    }
}
