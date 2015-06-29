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
        /// Used to avoid constant reflection (perf)
        /// </summary>
        private static readonly ConcurrentDictionary<Type, PropertyInfo[]> PropCache = new ConcurrentDictionary<Type, PropertyInfo[]>(); 

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

            var refProperties = PropCache.GetOrAdd(inputType, type =>
                inputType.GetProperties()
                    .Where(x =>
                        //is not attributed with the ignore clone attribute
                        x.GetCustomAttribute<DoNotCloneAttribute>() == null
                            //reference type but not string
                        && x.PropertyType.IsValueType == false && x.PropertyType != typeof (string)
                            //settable
                        && x.CanWrite
                            //non-indexed
                        && x.GetIndexParameters().Any() == false)
                    .ToArray());

            foreach (var propertyInfo in refProperties)
            {
                if (TypeHelper.IsTypeAssignableFrom<IDeepCloneable>(propertyInfo.PropertyType))
                {
                    //this ref property is also deep cloneable so clone it
                    var result = (IDeepCloneable)propertyInfo.GetValue(input, null);

                    if (result != null)
                    {
                        //set the cloned value to the property
                        propertyInfo.SetValue(output, result.DeepClone(), null);
                    }
                }
                else if (TypeHelper.IsTypeAssignableFrom<IEnumerable>(propertyInfo.PropertyType)
                    && TypeHelper.IsTypeAssignableFrom<string>(propertyInfo.PropertyType) == false)
                {
                    IList newList;
                    if (propertyInfo.PropertyType.IsGenericType
                        && (propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IEnumerable<>)
                            || propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>)
                            || propertyInfo.PropertyType.GetGenericTypeDefinition() == typeof(IList<>)))
                    {
                        //if it is a IEnumerable<>, IList<T> or ICollection<> we'll use a List<>
                        var genericType = typeof(List<>).MakeGenericType(propertyInfo.PropertyType.GetGenericArguments());
                        newList = (IList)Activator.CreateInstance(genericType);
                    }
                    else if (propertyInfo.PropertyType.IsArray
                             || (propertyInfo.PropertyType.IsInterface && propertyInfo.PropertyType.IsGenericType == false))
                    {
                        //if its an array, we'll create a list to work with first and then convert to array later
                        //otherwise if its just a regular derivitave of IEnumerable, we can use a list too
                        newList = new List<object>();
                    }
                    else
                    {
                        //its a custom IEnumerable, we'll try to create it
                        try
                        {
                            var custom = Activator.CreateInstance(propertyInfo.PropertyType);
                            //if it's an IList we can work with it, otherwise we cannot
                            newList = custom as IList;
                            if (newList == null)
                            {
                                continue;
                            }
                        }
                        catch (Exception)
                        {
                            //could not create this type so we'll skip it
                            continue;
                        }
                    }

                    var enumerable = (IEnumerable)propertyInfo.GetValue(input, null);
                    if (enumerable == null) continue;

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

                    if (propertyInfo.PropertyType.IsArray)
                    {
                        //need to convert to array
                        var arr = (object[])Activator.CreateInstance(propertyInfo.PropertyType, newList.Count);
                        for (int i = 0; i < newList.Count; i++)
                        {
                            arr[i] = newList[i];
                        }
                        //set the cloned collection
                        propertyInfo.SetValue(output, arr, null);
                    }
                    else
                    {
                        //set the cloned collection
                        propertyInfo.SetValue(output, newList, null);
                    }

                }
            }
        }

    }
}