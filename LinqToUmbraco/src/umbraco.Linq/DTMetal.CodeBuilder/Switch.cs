using System;

namespace umbraco.Linq.DTMetal.CodeBuilder
{
    public class Switch
    {
        public Switch(object o)
        {
            Obj = o;
        }

        public object Obj { get; private set; }
    
        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <returns></returns>
        public Switch Case<T>(Action<T> a)
        {
            return Case<T>(o => true, a, false);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <param name="fallThrough">if set to <c>true</c> fall through to next case statement.</param>
        /// <returns></returns>
        public Switch Case<T>(Action<T> a, bool fallThrough)
        {
            return Case<T>(o => true, a, fallThrough);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="c">The funcation to eveluate against to object.</param>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <returns></returns>
        public Switch Case<T>(Func<T, bool> c, Action<T> a)
        {
            return Case<T>(c, a, false);
        }

        /// <summary>
        /// Case statement
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="c">The funcation to eveluate against to object.</param>
        /// <param name="a">Action method to execute for case evaluation</param>
        /// <param name="fallThrough">if set to <c>true</c> fall through to next case statement.</param>
        /// <returns></returns>
        public Switch Case<T>(Func<T, bool> c, Action<T> a, bool fallThrough)
        {
            if (this == null)
            {
                return null;
            }
            else
            {
                if (this.Obj is T)
                {
                    T t = (T)this.Obj;
                    if (c(t))
                    {
                        a(t);
                        return fallThrough ? this : null;
                    }
                }
            }

            return this;
        }

        /// <summary>
        /// Defaults case
        /// </summary>
        /// <typeparam name="T">Type of object</typeparam>
        /// <param name="a">Action to perform</param>
        /// <returns></returns>
        public Switch Default<T>(Action<T> a)
        {
            if (this == null)
            {
                return null;
            }
            else
            {
                a((T)this.Obj);
                return this;
            }
        }
    }
}
