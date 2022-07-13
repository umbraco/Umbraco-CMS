namespace Umbraco.Cms.Core.Configuration.UmbracoSettings;

public class CharacterReplacementEqualityComparer : IEqualityComparer<IChar>
{
    public bool Equals(IChar? x, IChar? y)
    {
        if (ReferenceEquals(x, y))
        {
            return true;
        }

        if (x is null)
        {
            return false;
        }

        if (y is null)
        {
            return false;
        }

        if (x.GetType() != y.GetType())
        {
            return false;
        }

        return x.Char == y.Char && x.Replacement == y.Replacement;
    }

    public int GetHashCode(IChar obj)
    {
        unchecked
        {
            return ((obj.Char != null ? obj.Char.GetHashCode() : 0) * 397) ^
                   (obj.Replacement != null ? obj.Replacement.GetHashCode() : 0);
        }
    }
}
