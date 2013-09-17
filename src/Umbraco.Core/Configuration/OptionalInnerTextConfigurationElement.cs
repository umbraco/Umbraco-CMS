namespace Umbraco.Core.Configuration
{
    /// <summary>
    /// This is used to supply optional/default values when using InnerTextConfigurationElement
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class OptionalInnerTextConfigurationElement<T> : InnerTextConfigurationElement<T>
    {
        private readonly InnerTextConfigurationElement<T> _wrapped;
        private readonly T _defaultValue;

        public OptionalInnerTextConfigurationElement(InnerTextConfigurationElement<T> wrapped, T defaultValue)
        {
            _wrapped = wrapped;
            _defaultValue = defaultValue;
        }

        public override T Value
        {
            get { return string.IsNullOrEmpty(_wrapped.RawValue) ? _defaultValue : _wrapped.Value; }
        }
    }
}