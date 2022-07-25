namespace Umbraco.Cms.Core;

/// <summary>
///     A custom equality comparer that excepts a delegate to do the comparison operation
/// </summary>
/// <typeparam name="T"></typeparam>
public class DelegateEqualityComparer<T> : IEqualityComparer<T>
{
    private readonly Func<T?, T?, bool> _equals;
    private readonly Func<T, int> _getHashcode;

    #region Implementation of IEqualityComparer<in T>

    public DelegateEqualityComparer(Func<T?, T?, bool> equals, Func<T, int> getHashcode)
    {
        _getHashcode = getHashcode;
        _equals = equals;
    }

    public static DelegateEqualityComparer<T> CompareMember<TMember>(Func<T?, TMember> memberExpression)
        where TMember : IEquatable<TMember> =>
        new DelegateEqualityComparer<T>(
            (x, y) => memberExpression.Invoke(x).Equals(memberExpression.Invoke(y)),
            x =>
            {
                TMember invoked = memberExpression.Invoke(x);
                return !ReferenceEquals(invoked, default(TMember)) ? invoked.GetHashCode() : 0;
            });

    /// <summary>
    ///     Determines whether the specified objects are equal.
    /// </summary>
    /// <returns>
    ///     true if the specified objects are equal; otherwise, false.
    /// </returns>
    /// <param name="x">The first object of type <paramref name="T" /> to compare.</param>
    /// <param name="y">The second object of type <paramref name="T" /> to compare.</param>
    public bool Equals(T? x, T? y) => _equals.Invoke(x, y);

    /// <summary>
    ///     Returns a hash code for the specified object.
    /// </summary>
    /// <returns>
    ///     A hash code for the specified object.
    /// </returns>
    /// <param name="obj">The <see cref="T:System.Object" /> for which a hash code is to be returned.</param>
    /// <exception cref="T:System.ArgumentNullException">
    ///     The type of <paramref name="obj" /> is a reference type and
    ///     <paramref name="obj" /> is null.
    /// </exception>
    public int GetHashCode(T obj) => _getHashcode.Invoke(obj);

    #endregion
}
