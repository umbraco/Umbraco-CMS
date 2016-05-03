namespace Umbraco.Core
{
    using System;
    using System.Linq;
    using System.Reflection;

    using CallingConventions = System.Reflection.CallingConventions;

    /// <summary>
	/// Helper methods for Activation
	/// </summary>
	internal static class ActivatorHelper
	{
		/// <summary>
		/// Creates an instance of a type using that type's default constructor.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <returns></returns>
		public static T CreateInstance<T>() where T : class, new()
		{
		    return new ActivatorServiceProvider().GetService(typeof (T)) as T;
		}


        /// <summary>
        /// Creates an instance of a type using a constructor with specific arguments
        /// </summary>
        /// <typeparam name="T">The <see cref="Type"/> or base class</typeparam>
        /// <param name="type">The type of the object to be instantiated</param>
        /// <param name="constructorArgumentValues">Object array containing constructor arguments</param>
        /// <returns>The result of the <see cref="Attempt{T}"/> to instantiate the object</returns>
        internal static Attempt<T> CreateInstance<T>(Type type, object[] constructorArgumentValues) where T : class
        {
            if (type == null || constructorArgumentValues == null)
            {
                return Attempt<T>.Fail(new NullReferenceException("Failed to create Type due to null Type or null constructor args"));
            }
                
            const BindingFlags BindingFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

            var constructorArgumentTypes = constructorArgumentValues.Select(value => value.GetType()).ToList();

            var constructor = type.GetConstructor(BindingFlags, null, CallingConventions.Any, constructorArgumentTypes.ToArray(), null);

            try
            {
                var obj = constructor.Invoke(constructorArgumentValues);
                return (obj is T)
                           ? Attempt<T>.Succeed(obj as T)
                           : Attempt<T>.Fail(
                               new InvalidCastException("Object invoked was not of expected type: " + typeof(T).Name));
            }
            catch (Exception ex)
            {
                return Attempt<T>.Fail(ex);
            }
        }
    }
}