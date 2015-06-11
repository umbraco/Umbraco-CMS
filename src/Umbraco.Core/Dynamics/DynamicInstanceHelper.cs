using System;
using System.Collections;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;
using System.Text;
using Umbraco.Core.Cache;
using Umbraco.Core.Logging;

namespace Umbraco.Core.Dynamics
{
	/// <summary>
	/// A helper class to try invoke members, find properties, etc...
	/// </summary>
	internal class DynamicInstanceHelper
	{

		internal class TryInvokeMemberResult
		{
			public object ObjectResult { get; private set; }
			public TryInvokeMemberSuccessReason Reason { get; private set; }

			public TryInvokeMemberResult(object result, TryInvokeMemberSuccessReason reason)
			{
				ObjectResult = result;
				Reason = reason;
			}
		}

		internal enum TryInvokeMemberSuccessReason
		{
			FoundProperty,
			FoundMethod,
			FoundExtensionMethod
		}

	    /// <summary>
	    /// Attempts to invoke a member based on the dynamic instance
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="runtimeCache"></param>
	    /// <param name="thisObject">The object instance to invoke the extension method for</param>
	    /// <param name="binder"></param>
	    /// <param name="args"></param>
	    /// <returns></returns>
	    /// <remarks>
	    /// First tries to find a property with the binder name, if that fails it will try to find a static or instance method
	    /// on the object that matches the binder name
	    /// </remarks>
	    public static Attempt<TryInvokeMemberResult> TryInvokeMember<T>(IRuntimeCacheProvider runtimeCache, T thisObject, InvokeMemberBinder binder, object[] args)
		{
            return TryInvokeMember<T>(runtimeCache, thisObject, binder, args, null);
		}

	    /// <summary>
	    /// Attempts to invoke a member based on the dynamic instance
	    /// </summary>
	    /// <typeparam name="T"></typeparam>
	    /// <param name="runtimeCache"></param>
	    /// <param name="thisObject">The object instance to invoke the extension method for</param>
	    /// <param name="binder"></param>
	    /// <param name="args"></param>
	    /// <param name="findExtensionMethodsOnTypes">The types to scan for extension methods </param>
	    /// <returns></returns>
	    /// <remarks>
	    /// First tries to find a property with the binder name, if that fails it will try to find a static or instance method
	    /// on the object that matches the binder name, if that fails it will then attempt to invoke an extension method
	    /// based on the binder name and the extension method types to scan.
	    /// </remarks>
	    public static Attempt<TryInvokeMemberResult> TryInvokeMember<T>(
            IRuntimeCacheProvider runtimeCache,
            T thisObject, 
			InvokeMemberBinder binder, 
			object[] args, 
			IEnumerable<Type> findExtensionMethodsOnTypes)
		{
			//TODO: We MUST cache the result here, it is very expensive to keep finding extension methods!
			object result;
			try
			{
				//Property?
				result = typeof(T).InvokeMember(binder.Name,
				                                BindingFlags.Instance |
				                                BindingFlags.Public |
				                                BindingFlags.GetProperty,
				                                null,
				                                thisObject,
				                                args);
				return Attempt.Succeed(new TryInvokeMemberResult(result, TryInvokeMemberSuccessReason.FoundProperty));
			}
			catch (MissingMethodException)
			{
				try
				{
					//Static or Instance Method?
					result = typeof(T).InvokeMember(binder.Name,
					                                BindingFlags.Instance |
					                                BindingFlags.Public |
					                                BindingFlags.Static |
					                                BindingFlags.InvokeMethod,
					                                null,
					                                thisObject,
					                                args);
					return Attempt.Succeed(new TryInvokeMemberResult(result, TryInvokeMemberSuccessReason.FoundMethod));
				}
				catch (MissingMethodException)
				{
					if (findExtensionMethodsOnTypes != null)
					{
						try
						{
                            result = FindAndExecuteExtensionMethod(runtimeCache, thisObject, args, binder.Name, findExtensionMethodsOnTypes);
							return Attempt.Succeed(new TryInvokeMemberResult(result, TryInvokeMemberSuccessReason.FoundExtensionMethod));
						}
						catch (TargetInvocationException ext)
						{
							//don't log here, we return this exception because the caller may need to do something specific when
							//this exception occurs.
						    var mresult = new TryInvokeMemberResult(null, TryInvokeMemberSuccessReason.FoundExtensionMethod);
							return Attempt<TryInvokeMemberResult>.Fail(mresult, ext);
						}
						catch (Exception ex)
						{
                            var sb = new StringBuilder();
                            sb.AppendFormat("An error occurred finding and executing extension method \"{0}\" ", binder.Name);
                            sb.AppendFormat("for type \"{0}\". ", typeof (T));
                            sb.Append("Types searched for extension methods were ");
                            sb.Append(string.Join(", ", findExtensionMethodsOnTypes));
                            sb.Append(".");
							LogHelper.Error<DynamicInstanceHelper>(sb.ToString(), ex);
                            var mresult = new TryInvokeMemberResult(null, TryInvokeMemberSuccessReason.FoundExtensionMethod);
                            return Attempt<TryInvokeMemberResult>.Fail(mresult, ex);
						}	
					}
					return Attempt<TryInvokeMemberResult>.Fail();
				}
			}
			catch (Exception ex)
			{
				LogHelper.Error<DynamicInstanceHelper>("An unhandled exception occurred in method TryInvokeMember", ex);
				return Attempt<TryInvokeMemberResult>.Fail(ex);
			}
		}

	    /// <summary>
	    /// Attempts to find an extension method that matches the name and arguments based on scanning the Type's passed in
	    /// to the findMethodsOnTypes parameter
	    /// </summary>
	    /// <param name="runtimeCache"></param>
	    /// <param name="thisObject">The instance object to execute the extension method for</param>
	    /// <param name="args"></param>
	    /// <param name="name"></param>
	    /// <param name="findMethodsOnTypes"></param>
	    /// <returns></returns>
	    internal static object FindAndExecuteExtensionMethod<T>(
            IRuntimeCacheProvider runtimeCache,
            T thisObject, 
			object[] args, 
			string name, 
			IEnumerable<Type> findMethodsOnTypes)
		{
			object result = null;
			
			//find known extension methods that match the first type in the list
			MethodInfo toExecute = null;
			foreach (var t in findMethodsOnTypes)
			{
                toExecute = ExtensionMethodFinder.FindExtensionMethod(runtimeCache, t, args, name, false);
				if (toExecute != null)
					break;
			}

			if (toExecute != null)
			{
				var genericArgs = (new[] { (object)thisObject }).Concat(args);

                // else we'd get an exception w/ message "Late bound operations cannot
                // be performed on types or methods for which ContainsGenericParameters is true."
                // because MakeGenericMethod must be used to obtain an actual method that can run
                if (toExecute.ContainsGenericParameters)
                    throw new InvalidOperationException("Method contains generic parameters, something's wrong.");
                
				result = toExecute.Invoke(null, genericArgs.ToArray());	
			}
			else
			{
				throw new MissingMethodException(typeof(T).FullName, name);
			}			
			return result;
		}

	}
}