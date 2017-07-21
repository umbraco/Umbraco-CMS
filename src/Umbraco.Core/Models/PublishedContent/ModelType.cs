using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;

namespace Umbraco.Core.Models.PublishedContent
{
    // create a simple model type:
    //  ModelType.For("alias")
    // use in a generic type:
    //  typeof (IEnumerable<>).MakeGenericType(ModelType.For("alias"))
    // use in an array:
    //  ModelType.For("alias").MakeArrayType()

    public class ModelType : Type
    {
        private ModelType(string contentTypeAlias)
        {
            ContentTypeAlias = contentTypeAlias;
            Name = "{" + ContentTypeAlias + "}";
        }

        public string ContentTypeAlias { get; }

        public override string ToString()
            => Name;

        public static ModelType For(string alias)
            => new ModelType(alias);

        public static Type Map(Type type, Dictionary<string, Type> modelTypes)
        {
            if (type is ModelType modelType)
            {
                if (modelTypes.TryGetValue(modelType.ContentTypeAlias, out Type actualType))
                    return actualType;
                throw new InvalidOperationException($"Don't know how to map ModelType with content type alias \"{modelType.ContentTypeAlias}\".");
            }

            if (type is ModelTypeArrayType arrayType)
            {
                if (modelTypes.TryGetValue(arrayType.ContentTypeAlias, out Type actualType))
                    return actualType.MakeArrayType();
                throw new InvalidOperationException($"Don't know how to map ModelType with content type alias \"{arrayType.ContentTypeAlias}\".");
            }

            if (type.IsGenericType == false)
                return type;

            var args = type.GetGenericArguments().Select(x => Map(x, modelTypes)).ToArray();

            return type.GetGenericTypeDefinition().MakeGenericType(args);
        }

        public static bool Equals(Type t1, Type t2)
        {
            if (t1 == t2)
                return true;

            if (t1 is ModelType m1 && t2 is ModelType m2)
                return m1.ContentTypeAlias == m2.ContentTypeAlias;

            if (t1 is ModelTypeArrayType a1 && t2 is ModelTypeArrayType a2)
                return a1.ContentTypeAlias == a2.ContentTypeAlias;

            if (t1.IsGenericType == false || t2.IsGenericType == false)
                return false;

            var args1 = t1.GetGenericArguments();
            var args2 = t2.GetGenericArguments();
            if (args1.Length != args2.Length) return false;

            for (var i = 0; i < args1.Length; i++)
            {
                // ReSharper disable once CheckForReferenceEqualityInstead.2
                if (Equals(args1[i], args2[i]) == false) return false;
            }

            return true;
        }

        protected override TypeAttributes GetAttributeFlagsImpl()
            => TypeAttributes.Class;

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            => Array.Empty<ConstructorInfo>();

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override Type[] GetInterfaces()
            => Array.Empty<Type>();

        public override Type GetInterface(string name, bool ignoreCase)
            => null;

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            => Array.Empty<EventInfo>();

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            => null;

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            => Array.Empty<Type>();

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
            => null;

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            => Array.Empty<PropertyInfo>();

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            => Array.Empty<MethodInfo>();

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            => Array.Empty<FieldInfo>();

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            => null;

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            => Array.Empty<MemberInfo>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => Array.Empty<object>();

        public override object[] GetCustomAttributes(bool inherit)
            => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type GetElementType()
            => null;

        protected override bool HasElementTypeImpl()
            => false;

        protected override bool IsArrayImpl()
            => false;

        protected override bool IsByRefImpl()
            => false;

        protected override bool IsPointerImpl()
            => false;

        protected override bool IsPrimitiveImpl()
            => false;

        protected override bool IsCOMObjectImpl()
            => false;

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override Type UnderlyingSystemType => this;
        public override Type BaseType => null;

        public override string Name { get; }
        public override Guid GUID { get; } = Guid.NewGuid();
        public override Module Module => throw new NotSupportedException();
        public override Assembly Assembly => throw new NotSupportedException();
        public override string FullName => Name;
        public override string Namespace => string.Empty;
        public override string AssemblyQualifiedName => Name;

        public override Type MakeArrayType()
        {
            return new ModelTypeArrayType(this);
        }
    }

    internal class ModelTypeArrayType : Type
    {
        private readonly Type _elementType;

        public ModelTypeArrayType(ModelType type)
        {
            _elementType = type;
            ContentTypeAlias = type.ContentTypeAlias;
            Name = "{" + type.ContentTypeAlias + "}[*]";
        }

        public string ContentTypeAlias { get; }

        public override string ToString()
            => Name;

        protected override TypeAttributes GetAttributeFlagsImpl()
            => TypeAttributes.Class;

        public override ConstructorInfo[] GetConstructors(BindingFlags bindingAttr)
            => Array.Empty<ConstructorInfo>();

        protected override ConstructorInfo GetConstructorImpl(BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override Type[] GetInterfaces()
            => Array.Empty<Type>();

        public override Type GetInterface(string name, bool ignoreCase)
            => null;

        public override EventInfo[] GetEvents(BindingFlags bindingAttr)
            => Array.Empty<EventInfo>();

        public override EventInfo GetEvent(string name, BindingFlags bindingAttr)
            => null;

        public override Type[] GetNestedTypes(BindingFlags bindingAttr)
            => Array.Empty<Type>();

        public override Type GetNestedType(string name, BindingFlags bindingAttr)
            => null;

        public override PropertyInfo[] GetProperties(BindingFlags bindingAttr)
            => Array.Empty<PropertyInfo>();

        protected override PropertyInfo GetPropertyImpl(string name, BindingFlags bindingAttr, Binder binder, Type returnType, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override MethodInfo[] GetMethods(BindingFlags bindingAttr)
            => Array.Empty<MethodInfo>();

        protected override MethodInfo GetMethodImpl(string name, BindingFlags bindingAttr, Binder binder, CallingConventions callConvention, Type[] types, ParameterModifier[] modifiers)
            => null;

        public override FieldInfo[] GetFields(BindingFlags bindingAttr)
            => Array.Empty<FieldInfo>();

        public override FieldInfo GetField(string name, BindingFlags bindingAttr)
            => null;

        public override MemberInfo[] GetMembers(BindingFlags bindingAttr)
            => Array.Empty<MemberInfo>();

        public override object[] GetCustomAttributes(Type attributeType, bool inherit)
            => Array.Empty<object>();

        public override object[] GetCustomAttributes(bool inherit)
            => Array.Empty<object>();

        public override bool IsDefined(Type attributeType, bool inherit)
            => false;

        public override Type GetElementType()
            => _elementType;

        protected override bool HasElementTypeImpl()
            => true;

        protected override bool IsArrayImpl()
            => true;

        protected override bool IsByRefImpl()
            => false;

        protected override bool IsPointerImpl()
            => false;

        protected override bool IsPrimitiveImpl()
            => false;

        protected override bool IsCOMObjectImpl()
            => false;

        public override object InvokeMember(string name, BindingFlags invokeAttr, Binder binder, object target, object[] args, ParameterModifier[] modifiers, CultureInfo culture, string[] namedParameters)
        {
            throw new NotSupportedException();
        }

        public override Type UnderlyingSystemType => this;
        public override Type BaseType => null;

        public override string Name { get; }
        public override Guid GUID { get; } = Guid.NewGuid();
        public override Module Module => throw new NotSupportedException();
        public override Assembly Assembly => throw new NotSupportedException();
        public override string FullName => Name;
        public override string Namespace => string.Empty;
        public override string AssemblyQualifiedName => Name;

        public override int GetArrayRank()
            => 1;
    }
}
