using System.Web;

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

        public HtmlString IsFirst(string valueIfTrue)
        {
            return IsFirst(valueIfTrue, string.Empty);
        }

        public HtmlString IsFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsFirst() ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotFirst()
        {
            return IsFirst() == false;
        }

        public HtmlString IsNotFirst(string valueIfTrue)
        {
            return IsNotFirst(valueIfTrue, string.Empty);
        }

        public HtmlString IsNotFirst(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsNotFirst() ? valueIfTrue : valueIfFalse);
        }

        public bool IsIndex(int index)
        {
            return Index == index;
        }

        public HtmlString IsIndex(int index, string valueIfTrue)
        {
            return IsIndex(index, valueIfTrue, string.Empty);
        }

        public HtmlString IsIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsIndex(index) ? valueIfTrue : valueIfFalse);
        }

        public bool IsModZero(int modulus)
        {
            return Index % modulus == 0;
        }

        public HtmlString IsModZero(int modulus, string valueIfTrue)
        {
            return IsModZero(modulus, valueIfTrue, string.Empty);
        }

        public HtmlString IsModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotModZero(int modulus)
        {
            return IsModZero(modulus) == false;
        }

        public HtmlString IsNotModZero(int modulus, string valueIfTrue)
        {
            return IsNotModZero(modulus, valueIfTrue, string.Empty);
        }

        public HtmlString IsNotModZero(int modulus, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsNotModZero(modulus) ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotIndex(int index)
        {
            return IsIndex(index) == false;
        }

        public HtmlString IsNotIndex(int index, string valueIfTrue)
        {
            return IsNotIndex(index, valueIfTrue, string.Empty);
        }

        public HtmlString IsNotIndex(int index, string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsNotIndex(index) ? valueIfTrue : valueIfFalse);
        }

        public bool IsLast()
        {
            return Index == TotalCount - 1;
        }

        public HtmlString IsLast(string valueIfTrue)
        {
            return IsLast(valueIfTrue, string.Empty);
        }

        public HtmlString IsLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsLast() ? valueIfTrue : valueIfFalse);
        }

        public bool IsNotLast()
        {
            return IsLast() == false;
        }

        public HtmlString IsNotLast(string valueIfTrue)
        {
            return IsNotLast(valueIfTrue, string.Empty);
        }

        public HtmlString IsNotLast(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsNotLast() ? valueIfTrue : valueIfFalse);
        }

        public bool IsEven()
        {
            return Index % 2 == 0;
        }

        public HtmlString IsEven(string valueIfTrue)
        {
            return IsEven(valueIfTrue, string.Empty);
        }

        public HtmlString IsEven(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsEven() ? valueIfTrue : valueIfFalse);
        }

        public bool IsOdd()
        {
            return Index % 2 == 1;
        }

        public HtmlString IsOdd(string valueIfTrue)
        {
            return IsOdd(valueIfTrue, string.Empty);
        }

        public HtmlString IsOdd(string valueIfTrue, string valueIfFalse)
        {
            return new HtmlString(IsOdd() ? valueIfTrue : valueIfFalse);
        }
    }
}
