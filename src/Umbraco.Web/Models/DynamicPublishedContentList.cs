using System;
using System.Collections.Generic;
using System.Linq;
using System.Dynamic;
using Umbraco.Core;
using Umbraco.Core.Dynamics;
using System.Collections;
using System.Reflection;
using Umbraco.Core.Models;
using Umbraco.Web.Dynamics;

namespace Umbraco.Web.Models
{
    /// <summary>
    /// A collection of DynamicPublishedContent items
    /// </summary>
    /// <remarks>
    /// Implements many of the dynamic methods required for execution against this list. It also ensures
    /// that the correct OwnersCollection properties is assigned to the underlying PublishedContentBase object
    /// of the DynamicPublishedContent item (so long as the IPublishedContent item is actually PublishedContentBase).
    /// All relates to this issue here: http://issues.umbraco.org/issue/U4-1797
    /// </remarks>
    public class DynamicPublishedContentList : DynamicObject, IEnumerable<DynamicPublishedContent>, IEnumerable<IPublishedContent>
    {
		internal List<DynamicPublishedContent> Items { get; set; }

        public DynamicPublishedContentList()
        {
            Items = new List<DynamicPublishedContent>();
        }
        public DynamicPublishedContentList(IEnumerable<DynamicPublishedContent> items)
        {
            var list = items.ToList();
            //set the owners list for each item
            list.ForEach(x => SetOwnersList(x, this));
            Items = list;
        }

        public DynamicPublishedContentList(IEnumerable<IPublishedContent> items)
        {
            var list = items.Select(x => new DynamicPublishedContent(x)).ToList();
            //set the owners list for each item
            list.ForEach(x => SetOwnersList(x, this));
            Items = list;
        }

        private static void SetOwnersList(IPublishedContent content, IEnumerable<DynamicPublishedContent> list)
        {
            var publishedContentBase = content as IOwnerCollectionAware<IPublishedContent>;
            if (publishedContentBase != null)
            {
                publishedContentBase.OwnersCollection = list;
            }            
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
			//TODO: Nowhere here are we checking if args is the correct length!

			//NOTE: For many of these we could actually leave them out since we are executing custom extension methods and because
			// we implement IEnumerable<T> they will execute just fine, however, to do that will be quite a bit slower than checking here.

			var firstArg = args.FirstOrDefault();
			//this is to check for 'DocumentTypeAlias' vs 'NodeTypeAlias' for compatibility
			if (firstArg != null && firstArg.ToString().InvariantStartsWith("NodeTypeAlias"))
			{
				firstArg = "DocumentTypeAlias" + firstArg.ToString().Substring("NodeTypeAlias".Length);
			}

            var name = binder.Name;
			if (name == "Single")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] {} : args.Skip(1).ToArray();
				var single = this.Single<DynamicPublishedContent>(predicate, values);				
				result = new DynamicPublishedContent(single);				
				return true;
			}
			if (name == "SingleOrDefault")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] { } : args.Skip(1).ToArray();
				var single = this.SingleOrDefault<DynamicPublishedContent>(predicate, values);
				if (single == null)
					result = new DynamicNull();
				else
					result = new DynamicPublishedContent(single);
				return true;
			}
			if (name == "First")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] { } : args.Skip(1).ToArray();
				var first = this.First<DynamicPublishedContent>(predicate, values);
				result = new DynamicPublishedContent(first);
				return true;
			}
			if (name == "FirstOrDefault")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] { } : args.Skip(1).ToArray();
				var first = this.FirstOrDefault<DynamicPublishedContent>(predicate, values);
				if (first == null)
					result = new DynamicNull();
				else
					result = new DynamicPublishedContent(first);
				return true;
			}
			if (name == "Last")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] { } : args.Skip(1).ToArray();
				var last = this.Last<DynamicPublishedContent>(predicate, values);
				result = new DynamicPublishedContent(last);
				return true;
			}
			if (name == "LastOrDefault")
			{
				string predicate = firstArg == null ? "" : firstArg.ToString();
				var values = predicate.IsNullOrWhiteSpace() ? new object[] { } : args.Skip(1).ToArray();
				var last = this.LastOrDefault<DynamicPublishedContent>(predicate, values);
				if (last == null)
					result = new DynamicNull();
				else
					result = new DynamicPublishedContent(last);
				return true;
			}
            if (name == "Where")
            {
				string predicate = firstArg.ToString();
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
				result = new DynamicPublishedContentList(this.OrderBy<DynamicPublishedContent>(firstArg.ToString()).ToList());
                return true;
            }
			if (name == "Take")
			{
				result = new DynamicPublishedContentList(this.Take<DynamicPublishedContent>((int)firstArg));
				return true;
			}
			if (name == "Skip")
			{
                result = new DynamicPublishedContentList(this.Skip<DynamicPublishedContent>((int)firstArg));
				return true;
			}
        	if (name == "InGroupsOf")
            {
                int groupSize = 0;
				if (int.TryParse(firstArg.ToString(), out groupSize))
                {
                    result = InGroupsOf(groupSize);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupedInto")
            {
                int groupCount = 0;
				if (int.TryParse(firstArg.ToString(), out groupCount))
                {
                    result = GroupedInto(groupCount);
                    return true;
                }
                result = new DynamicNull();
                return true;
            }
            if (name == "GroupBy")
            {
				result = GroupBy(firstArg.ToString());
                return true;
            }
            if (name == "Average" || name == "Min" || name == "Max" || name == "Sum")
            {
                result = Aggregate(args, name);
                return true;
            }
            if (name == "Union")
            {
				if ((firstArg as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Union(firstArg as IEnumerable<DynamicPublishedContent>));
                    return true;
                }
				if ((firstArg as DynamicPublishedContentList) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Union((firstArg as DynamicPublishedContentList).Items));				
                    return true;
                }
            }
            if (name == "Except")
            {
				if ((firstArg as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Except(firstArg as IEnumerable<DynamicPublishedContent>, new DynamicPublishedContentIdEqualityComparer()));					
                    return true;
                }
            }
            if (name == "Intersect")
            {
				if ((firstArg as IEnumerable<DynamicPublishedContent>) != null)
                {
					result = new DynamicPublishedContentList(this.Items.Intersect(firstArg as IEnumerable<DynamicPublishedContent>, new DynamicPublishedContentIdEqualityComparer()));					
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

			//ok, now lets try to match by member, property, extensino method
			var attempt = DynamicInstanceHelper.TryInvokeMember(this, binder, args, new[]
				{
					typeof (IEnumerable<DynamicPublishedContent>),
					typeof (DynamicPublishedContentList)
				});

			if (attempt.Success)
			{
				result = attempt.Result.ObjectResult;

				//need to check the return type and possibly cast if result is from an extension method found
				if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod)
				{
					//we don't need to cast if the result is already DynamicPublishedContentList
					if (attempt.Result.ObjectResult != null && (!(attempt.Result.ObjectResult is DynamicPublishedContentList)))
					{
						if (attempt.Result.ObjectResult is IPublishedContent)
						{
							result = new DynamicPublishedContent((IPublishedContent)attempt.Result.ObjectResult);
						}
						else if (attempt.Result.ObjectResult is IEnumerable<DynamicPublishedContent>)
						{
							result = new DynamicPublishedContentList((IEnumerable<DynamicPublishedContent>)attempt.Result.ObjectResult);
						}
						else if (attempt.Result.ObjectResult is IEnumerable<IPublishedContent>)
						{
							result = new DynamicPublishedContentList((IEnumerable<IPublishedContent>)attempt.Result.ObjectResult);
						}						
					}
				}
				return true;
			}

			//this is the result of an extension method execution gone wrong so we return dynamic null
			if (attempt.Result.Reason == DynamicInstanceHelper.TryInvokeMemberSuccessReason.FoundExtensionMethod
				&& attempt.Exception != null && attempt.Exception is TargetInvocationException)
			{
				result = new DynamicNull();
				return true;
			}

			result = null;
			return false;

        }
        private T Aggregate<T>(IEnumerable<T> data, string name) where T : struct
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
		
		public T Single<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace() 
				? ((IQueryable<T>) Items.AsQueryable()).Single() 
				: Where<T>(predicate, values).Single();
		}

	    public T SingleOrDefault<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace()
				? ((IQueryable<T>)Items.AsQueryable()).SingleOrDefault()
				: Where<T>(predicate, values).SingleOrDefault();
		}
		public T First<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace()
				? ((IQueryable<T>)Items.AsQueryable()).First()
				: Where<T>(predicate, values).First();
		}
		public T FirstOrDefault<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace()
				? ((IQueryable<T>)Items.AsQueryable()).FirstOrDefault()
				: Where<T>(predicate, values).FirstOrDefault();
		}
		public T Last<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace()
				? ((IQueryable<T>)Items.AsQueryable()).Last()
				: Where<T>(predicate, values).Last();
		}
		public T LastOrDefault<T>(string predicate, params object[] values)
		{
			return predicate.IsNullOrWhiteSpace()
				? ((IQueryable<T>)Items.AsQueryable()).LastOrDefault()
				: Where<T>(predicate, values).LastOrDefault();
		}
        public IQueryable<T> Where<T>(string predicate, params object[] values)
        {
            return ((IQueryable<T>)Items.AsQueryable()).Where(predicate, values);
        }
        public IQueryable<T> OrderBy<T>(string key)
        {
			return ((IQueryable<T>)Items.AsQueryable()).OrderBy<T>(key, () => typeof(DynamicPublishedContentListOrdering));
        }
        public DynamicGrouping GroupBy(string key)
        {
            var group = new DynamicGrouping(this, key);
            return group;
        }
        public DynamicGrouping GroupedInto(int groupCount)
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
        public DynamicGrouping InGroupsOf(int groupSize)
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
	        return DynamicQueryable.Select(Items.AsQueryable(), predicate, values);
        }

        /// <summary>
        /// Allows the adding of an item from the collection
        /// </summary>
        /// <param name="publishedContent"></param>
        public void Add(DynamicPublishedContent publishedContent)
        {
            SetOwnersList(publishedContent, this);
            this.Items.Add(publishedContent);
        }

        /// <summary>
        /// Allows the removal of an item from the collection
        /// </summary>
        /// <param name="publishedContent"></param>
        public void Remove(DynamicPublishedContent publishedContent)
        {
            if (this.Items.Contains(publishedContent))
            {
                //set owners list to null
                SetOwnersList(publishedContent, null);
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

        IEnumerator<IPublishedContent> IEnumerable<IPublishedContent>.GetEnumerator()
        {
            return GetEnumerator();
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
