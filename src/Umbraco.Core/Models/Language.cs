﻿using System;
using System.Globalization;
using System.Runtime.Serialization;
using System.Threading;
using Umbraco.Core.Configuration;
using Umbraco.Core.Models.Entities;

namespace Umbraco.Core.Models
{
    /// <summary>
    /// Represents a Language.
    /// </summary>
    [Serializable]
    [DataContract(IsReference = true)]
    public class Language : EntityBase, ILanguage
    {
        private string _isoCode;
        private string _cultureName;
        private bool _isDefaultVariantLanguage;
        private bool _mandatory;
        private int? _fallbackLanguageId;

        public Language(string isoCode)
        {
            IsoCode = isoCode;
        }

        /// <inheritdoc />
        [DataMember]
        public string IsoCode
        {
            get => _isoCode;
            set => SetPropertyValueAndDetectChanges(value, ref _isoCode, nameof(IsoCode));
        }

        /// <inheritdoc />
        [DataMember]
        public string CultureName
        {
            // CultureInfo.DisplayName is the name in the installed .NET language
            //            .NativeName is the name in culture info's language
            //            .EnglishName is the name in English
            //
            // there is no easy way to get the name in a specified culture (which would need to be installed on the server)
            // this works:
            //   var rm = new ResourceManager("mscorlib", typeof(int).Assembly);
            //   var name = rm.GetString("Globalization.ci_" + culture.Name, displayCulture);
            // but can we rely on it?
            //
            // and... DisplayName is captured and cached in culture infos returned by GetCultureInfo(), using
            // the value for the current thread culture at the moment it is first retrieved - whereas creating
            // a new CultureInfo() creates a new instance, which _then_ can get DisplayName again in a different
            // culture
            //
            // I assume that, on a site, all language names should be in the SAME language, in DB,
            // and that would be the Umbraco.Core.DefaultUILanguage (app setting) - BUT if by accident
            // ANY culture has been retrieved with another current thread culture - it's now corrupt
            //
            // so, the logic below ensures that the name always end up being the correct name
            // see also LanguageController.GetAllCultures which is doing the same
            //
            // all this, including the ugly settings injection, because se store language names in db,
            // otherwise it would be ok to simply return new CultureInfo(IsoCode).DisplayName to get the name
            // in whatever culture is current - we should not do it, see task #3623
            //
            // but then, some tests that compare audit strings (for culture names) would need to be fixed

            get
            {
                if (_cultureName != null) return _cultureName;

                // capture
                var threadUiCulture = Thread.CurrentThread.CurrentUICulture;

                try
                {
                    var globalSettings = (IGlobalSettings) Composing.Current.Factory.GetInstance(typeof(IGlobalSettings));
                    var defaultUiCulture = CultureInfo.GetCultureInfo(globalSettings.DefaultUILanguage);
                    Thread.CurrentThread.CurrentUICulture = defaultUiCulture;

                    // get name - new-ing an instance to get proper display name
                    return new CultureInfo(IsoCode).DisplayName;
                }
                finally
                {
                    // restore
                    Thread.CurrentThread.CurrentUICulture = threadUiCulture;
                }
            }

            set => SetPropertyValueAndDetectChanges(value, ref _cultureName, nameof(CultureName));
        }

        /// <inheritdoc />
        [IgnoreDataMember]
        public CultureInfo CultureInfo => CultureInfo.GetCultureInfo(IsoCode);

        /// <inheritdoc />
        public bool IsDefault
        {
            get => _isDefaultVariantLanguage;
            set => SetPropertyValueAndDetectChanges(value, ref _isDefaultVariantLanguage, nameof(IsDefault));
        }

        /// <inheritdoc />
        public bool IsMandatory
        {
            get => _mandatory;
            set => SetPropertyValueAndDetectChanges(value, ref _mandatory, nameof(IsMandatory));
        }

        /// <inheritdoc />
        public int? FallbackLanguageId
        {
            get => _fallbackLanguageId;
            set => SetPropertyValueAndDetectChanges(value, ref _fallbackLanguageId, nameof(FallbackLanguageId));
        }
    }
}
