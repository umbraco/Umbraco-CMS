using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core.Models
{
    public static class DeepCloneHelper
    {
        /// <summary>
        /// Stores the metadata for the properties for a given type so we know how to create them
        /// </summary>
        private struct ClonePropertyInfo
        {
            public ClonePropertyInfo(PropertyInfo propertyInfo) : this()
            {
                if (propertyInfo == null) throw new ArgumentNullException("propertyInfo");
                PropertyInfo = propertyInfo;
            }

            public PropertyInfo PropertyInfo { get; private set; }
            public bool IsDeepCloneable { get; set; }
            public Type GenericListType { get; set; }           
            public bool IsList
            {
                get { return GenericListType != null; }
            }
        }

        /// <summary>
        /// Used to avoid constant reflection (perf)
        /// </summary>
        private static readonly ConcurrentDictionary<Type, ClonePropertyInfo[]> PropCache = new ConcurrentDictionary<Type, ClonePropertyInfo[]>();

        /// <summary>
        /// Used to deep clone any reference properties on the object (should be done after a MemberwiseClone for which the outcome is 'output')
        /// </summary>
        /// <param name="input"></param>
        /// <param name="output"></param>
        /// <returns></returns>        
        public static void DeepCloneRefProperties(IDeepCloneable input, IDeepCloneable output)
        {
            var inputType = input.GetType();
            var outputType = output.GetType();

            if (inputType != outputType)
            {
                throw new InvalidOperationException("Both the input and output types must be the same");
            }

            //get the property metadata from cache so we only have to figure this out once per type
            var refProperties = PropCache.GetOrAdd(inputType, type =>
                inputType.GetProperties()
                    .Select<PropertyInfo, ClonePropertyInfo?>(propertyInfo =>
                    {
                        if (
                            //is not attributed with the ignore clone attribute
                            propertyInfo.GetCustomAttribute<DoNotCloneAttribute>() != null
                            //reference type but not string
                            || propertyInfo.PropertyType.IsValueType || propertyInfo.PropertyType == typeof (string)
                            //settable
                            || propertyInfo.CanWrite == false
                            //non-indexed
                            || propertyInfo.GetIndexParameters().Any())
                        {
                            return null;
                        }
                        

                        if (TypeHelper.IsTypeAssignableFrom<IDeepCloneable>(propertyInfo.PropertyType))
                        {
                            return new ClonePropertyInfo(propertyInfo) { IsDeepCloneable = true };
                        }

                        if (TypeHelper.IsTypeAssignableFrom<IEnumerable>(propertyInfo.PropertyType)
                            && TypeHelper.IsTypeAssignableFrom<string>(propertyInfo.PropertyType) == false)
                        {
                            if (propertyInfo.PropertyType.IsGenericType
                                && (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                                    || propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                                    || propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)))
                            {
                                //if it is a IEnumerable<>, IList<T> or ICollection<> we'll use a List<>
                                var genericType = typeof(List<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments());
                                return new ClonePropertyInfo(propertyInfo) { GenericListType = genericType };
                            }
                            if (propertyInfo.PropertyType.IsArray
                                || (propertyInfo.PropertyType.IsInterface && propertyInfo.PropertyType.IsGenericType == false))
                            {
                                //if its an array, we'll create a list to work with first and then convert to array later
                                //otherwise if its just a regular derivitave of IEnumerable, we can use a list too
                                return new ClonePropertyInfo(propertyInfo) { GenericListType = typeof(List<object>) };
                            }
                            //skip instead of trying to create instance of abstract or interface
                            if (propertyInfo.PropertyType.IsAbstract || propertyInfo.PropertyType.IsInterface)
                            {
                                return null;
                            }

                            //its a custom IEnumerable, we'll try to create it
                            try
                            {
                                var custom = Activator.CreateInstance(propertyInfo.PropertyType);
                                //if it's an IList we can work with it, otherwise we cannot
                                var newList = custom as IList;
                                if (newList == null)
                                {
                                    return null;
                                }
                                return new ClonePropertyInfo(propertyInfo) {GenericListType = propertyInfo.PropertyType};
                            }
                            catch (Exception)
                            {
                                //could not create this type so we'll skip it
                                return null;
                            }
                        }
                        return new ClonePropertyInfo(propertyInfo);
                    })
                    .Where(x => x.HasValue)
                    .Select(x => x.Value)
                    .ToArray());

            foreach (var clonePropertyInfo in refProperties)
            {
                if (clonePropertyInfo.IsDeepCloneable)
                {
                    //this ref property is also deep cloneable so clone it
                    var result = (IDeepCloneable)clonePropertyInfo.PropertyInfo.GetValue(input, null);

                    if (result != null)
                    {
                        //set the cloned value to the property
                        clonePropertyInfo.PropertyInfo.SetValue(output, result.DeepClone(), null);
                    }
                }
                else if (clonePropertyInfo.IsList)
                {
                    var enumerable = (IEnumerable)clonePropertyInfo.PropertyInfo.GetValue(input, null);
                    if (enumerable == null) continue;

                    var newList = (IList)Activator.CreateInstance(clonePropertyInfo.GenericListType);

                    var isUsableType = true;

                    //now clone each item
                    foreach (var o in enumerable)
                    {
                        //first check if the item is deep cloneable and copy that way
                        var dc = o as IDeepCloneable;
                        if (dc != null)
                        {
                            newList.Add(dc.DeepClone());
                        }
                        else if (o is string || o.GetType().IsValueType)
                        {
                            //check if the item is a value type or a string, then we can just use it                         
                            newList.Add(o);
                        }
                        else
                        {
                            //this will occur if the item is not a string or value type or IDeepCloneable, in this case we cannot
                            // clone each element, we'll need to skip this property, people will have to manually clone this list
                            isUsableType = false;
                            break;
                        }
                    }

                    //if this was not usable, skip this property
                    if (isUsableType == false)
                    {
                        continue;
                    }

                    if (clonePropertyInfo.PropertyInfo.PropertyType.IsArray)
                    {
                        //need to convert to array
                        var arr = (object[])Activator.CreateInstance(clonePropertyInfo.PropertyInfo.PropertyType, newList.Count);
                        for (int i = 0; i < newList.Count; i++)
                        {
                            arr[i] = newList[i];
                        }
                        //set the cloned collection
                        clonePropertyInfo.PropertyInfo.SetValue(output, arr, null);
                    }
                    else
                    {
                        //set the cloned collection
                        clonePropertyInfo.PropertyInfo.SetValue(output, newList, null);
                    }

                }
            }
        }

    }
}