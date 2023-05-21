namespace Umbraco.Cms.Infrastructure.ModelsBuilder;

internal static class TypeExtensions
{
    /// <summary>
    ///     Creates a generic instance of a generic type with the proper actual type of an object.
    /// </summary>
    /// <param name="genericType">A generic type such as <c>Something{}</c></param>
    /// <param name="typeParmObj">An object whose type is used as generic type param.</param>
    /// <param name="ctorArgs">Arguments for the constructor.</param>
    /// <returns>A generic instance of the generic type with the proper type.</returns>
    /// <remarks>
    ///     Usage... typeof (Something{}).CreateGenericInstance(object1, object2, object3) will return
    ///     a Something{Type1} if object1.GetType() is Type1.
    /// </remarks>
    public static object? CreateGenericInstance(this Type genericType, object typeParmObj, params object[] ctorArgs)
    {
        Type type = genericType.MakeGenericType(typeParmObj.GetType());
        return Activator.CreateInstance(type, ctorArgs);
    }
}
