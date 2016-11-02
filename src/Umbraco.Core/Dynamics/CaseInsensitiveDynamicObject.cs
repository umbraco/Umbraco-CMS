using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core.Dynamics
{
    /// <summary>
    /// This will check enable dynamic access to properties and methods in a case insensitive manner
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <remarks>
    /// This works by using reflection on the type - the reflection lookup is lazy so it will not execute unless a dynamic method needs to be accessed
    /// </remarks>
    public abstract class CaseInsensitiveDynamicObject<T> : DynamicObject
        where T: class
    {
        /// <summary>
        /// Used for dynamic access for case insensitive property access
        /// </summary>`
        private static readonly Lazy<IDictionary<string, Func<T, object>>> CaseInsensitivePropertyAccess = new Lazy<IDictionary<string, Func<T, object>>>(() =>
        {
            var props = typeof(T).GetProperties(BindingFlags.Instance | BindingFlags.Public)
                .DistinctBy(x => x.Name);
            return props.Select(propInfo =>
            {
                var name = propInfo.Name.ToLowerInvariant();
                Func<T, object> getVal = propInfo.GetValue;
                return new KeyValuePair<string, Func<T, object>>(name, getVal);

            }).ToDictionary(x => x.Key, x => x.Value);
        });

        /// <summary>
        /// Used for dynamic access for case insensitive property access
        /// </summary>
        private static readonly Lazy<IDictionary<string, Tuple<ParameterInfo[], Func<T, object[], object>>>> CaseInsensitiveMethodAccess
            = new Lazy<IDictionary<string, Tuple<ParameterInfo[], Func<T, object[], object>>>>(() =>
            {
                var props = typeof(T).GetMethods(BindingFlags.Instance | BindingFlags.Public)
                    .Where(x => x.IsSpecialName == false && x.IsVirtual == false)
                    .DistinctBy(x => x.Name);
                return props.Select(methodInfo =>
                {
                    var name = methodInfo.Name.ToLowerInvariant();
                    Func<T, object[], object> getVal = methodInfo.Invoke;
                    var val = new Tuple<ParameterInfo[], Func<T, object[], object>>(methodInfo.GetParameters(), getVal);
                    return new KeyValuePair<string, Tuple<ParameterInfo[], Func<T, object[], object>>>(name, val);

                }).ToDictionary(x => x.Key, x => x.Value);
            });

        public override bool TryInvokeMember(InvokeMemberBinder binder, object[] args, out object result)
        {
            var name = binder.Name.ToLowerInvariant();
            if (CaseInsensitiveMethodAccess.Value.ContainsKey(name) == false)
                return base.TryInvokeMember(binder, args, out result);

            var val = CaseInsensitiveMethodAccess.Value[name];
            var parameters = val.Item1;
            var callback = val.Item2;
            var fullArgs = new List<object>(args);
            if (args.Length <= parameters.Length)
            {
                //need to fill them up if they're optional
                for (var i = args.Length; i < parameters.Length; i++)
                {
                    if (parameters[i].IsOptional)
                    {
                        fullArgs.Add(parameters[i].DefaultValue);
                    }
                }
                if (fullArgs.Count == parameters.Length)
                {
                    result = callback((T)(object)this, fullArgs.ToArray());
                    return true;
                }
            }
            return base.TryInvokeMember(binder, args, out result);
        }

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            var name = binder.Name.ToLowerInvariant();
            if (CaseInsensitivePropertyAccess.Value.ContainsKey(name) == false)
                return base.TryGetMember(binder, out result);

            result = CaseInsensitivePropertyAccess.Value[name]((T)(object)this);
            return true;
        }
    }
}