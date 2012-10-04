using System.Globalization;
using System.Reflection;
using Umbraco.Core.Models.EntityBase;

namespace Umbraco.Core.Models
{
    public class Language : Entity
    {
        private string _isoCode;
        private string _cultureName;

        public Language(string isoCode)
        {
            IsoCode = isoCode;
        }

        private static readonly PropertyInfo IsoCodeSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.IsoCode);
        private static readonly PropertyInfo CultureNameSelector = ExpressionHelper.GetPropertyInfo<Language, string>(x => x.CultureName);

        public string IsoCode
        {
            get { return _isoCode; }
            set
            {
                _isoCode = value;
                OnPropertyChanged(IsoCodeSelector);
            }
        }

        public string CultureName
        {
            get { return _cultureName; }
            set
            {
                _cultureName = value;
                OnPropertyChanged(CultureNameSelector);
            }
        }

        public CultureInfo CultureInfo
        {
            get { return CultureInfo.CreateSpecificCulture(IsoCode); }
        }
    }
}