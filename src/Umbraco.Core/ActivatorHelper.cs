namespace Umbraco.Core
{
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

	    
	}
}