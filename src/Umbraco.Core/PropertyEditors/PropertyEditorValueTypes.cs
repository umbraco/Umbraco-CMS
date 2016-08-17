using System;

namespace Umbraco.Core.PropertyEditors
{
    public static class PropertyEditorValueTypes
    {
        // mapped to DataTypeDatabaseType in DataTypeService.OverrideDatabaseTypeIfProvidedInPreValues
        // BUT what about those that are not mapped?
        //
        // also mapped to DataTypeDatabaseType in PropertyValueEditor
        // and this time the "+" are mapped

        public const string Date = "DATE"; // +Date

        public const string DateTime = "DATETIME"; // Date

        public const string Decimal = "DECIMAL"; // Decimal

        public const string Integer = "INT"; // Integer

        [Obsolete("Use Integer.", false)]
        public const string IntegerAlternative = "INTEGER"; // +Integer

        public const string Json = "JSON"; // +NText

        public const string Text = "TEXT"; // NText

        public const string Time = "TIME"; // +Date

        public const string String = "STRING"; // NVarchar

        public const string Xml = "XML"; // +NText
    }
}
