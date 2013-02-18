using System;

namespace umbraco.BusinessLogic.Utils
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
    ///				return Singleton{Form1}.Instance;
    ///			}
    ///		}
    ///	}
    /// </summary>
    /// <typeparam name="T">Any class that implements default constructor</typeparam>
    [Obsolete("Creating singletons should be done manually using the many different techiques available in .Net")]
    public static class Singleton<T> where T : new()
    {
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
