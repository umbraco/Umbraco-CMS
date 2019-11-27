namespace Umbraco.Core.Models.PublishedContent
{
    public class IndexedArrayItem<TContent>
    {
        public IndexedArrayItem(TContent content, int index)
        {
            Content = content;
            Index = index;
        }

        public TContent Content { get; }

        public int Index { get; }

        public int TotalCount { get; internal set; }

        public bool IsFirst()
        {
            return Index == 0;
        }

        public IHtmlEncodedString IsFirst(string valueIfTrue)
        {
            return IsFirst(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsFirst() ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotFirst()
        {
            return IsFirst() == false;
        }

        public IHtmlEncodedString IsNotFirst(string valueIfTrue)
        {
            return IsNotFirst(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsNotFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsNotFirst() ? valueIfTrue : valueIfFalse);
        }

        public bool IsIndex(int index)
        {
            return Index == index;
        }

        public IHtmlEncodedString IsIndex(int index, string valueIfTrue)
        {
            return IsIndex(index, valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsIndex(index) ? valueIfTrue : valueIfFalse);
        }

        public bool IsModZero(int modulus)
        {
            return Index % modulus == 0;
        }

        public IHtmlEncodedString IsModZero(int modulus, string valueIfTrue)
        {
            return IsModZero(modulus, valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotModZero(int modulus)
        {
            return IsModZero(modulus) == false;
        }

        public IHtmlEncodedString IsNotModZero(int modulus, string valueIfTrue)
        {
            return IsNotModZero(modulus, valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsNotModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotIndex(int index)
        {
            return IsIndex(index) == false;
        }

        public IHtmlEncodedString IsNotIndex(int index, string valueIfTrue)
        {
            return IsNotIndex(index, valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsNotIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsNotIndex(index) ? valueIfTrue : valueIfFalse);
        }

        public bool IsLast()
        {
            return Index == TotalCount - 1;
        }

        public IHtmlEncodedString IsLast(string valueIfTrue)
        {
            return IsLast(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsLast() ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotLast()
        {
            return IsLast() == false;
        }

        public IHtmlEncodedString IsNotLast(string valueIfTrue)
        {
            return IsNotLast(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsNotLast() ? valueIfTrue : valueIfFalse);
        }

        public bool IsEven()
        {
            return Index % 2 == 0;
        }

        public IHtmlEncodedString IsEven(string valueIfTrue)
        {
            return IsEven(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsEven(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsEven() ? valueIfTrue : valueIfFalse);
        }

        public bool IsOdd()
        {
            return Index % 2 == 1;
        }

        public IHtmlEncodedString IsOdd(string valueIfTrue)
        {
            return IsOdd(valueIfTrue, string.Empty);
        }

        public IHtmlEncodedString IsOdd(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlEncodedString(IsOdd() ? valueIfTrue : valueIfFalse);
        }
    }
}
