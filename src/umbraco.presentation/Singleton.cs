using System;

/// <changelog>
///   <item who="Esben" when="17. november 2006">Created</item>
/// </changelog>

namespace umbraco
{
	/// <summary>
	/// 
	/// Threadsafe Singleton best practice design pattern template
	/// 
	/// Sample:
	/// 
	/// public class Demo
	/// {
	///		public static Form1 instance1
	///		{
	///			get
	///			{
	///				return Singleton<Form1>.Instance;
	///			}
	///		}
	///	}
	/// </summary>
	/// <typeparam name="T">Any class that implements default constructor</typeparam>
	public sealed class Singleton<T> where T : new()
	{
		private Singleton()
		{
		}

		public static T Instance
		{
			get { return Nested.instance; }
		}

		private class Nested
		{
			// Explicit static constructor to tell C# compiler
			// not to mark type as beforefieldinit
			static Nested()
			{
			}

			internal static readonly T instance = new T();
		}
	}
}
