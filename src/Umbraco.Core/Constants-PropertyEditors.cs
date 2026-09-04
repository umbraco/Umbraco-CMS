using Umbraco.Cms.Core.PropertyEditors;

namespace Umbraco.Cms.Core;

public static partial class Constants
{
    /// <summary>
    ///     Defines property editors constants.
    /// </summary>
    public static class PropertyEditors
    {
        /// <summary>
        ///     Used to prefix generic properties that are internal content properties
        /// </summary>
        public const string InternalGenericPropertiesPrefix = "_umb_";

        /// <summary>
        ///     Contains legacy property editor aliases from previous Umbraco versions.
        /// </summary>
        public static class Legacy
        {
            /// <summary>
            ///     Defines legacy property editor aliases.
            /// </summary>
            public static class Aliases
            {
                /// <summary>
                ///     Legacy alias for the Textbox property editor.
                /// </summary>
                public const string Textbox = "Umbraco.Textbox";

                /// <summary>
                ///     Legacy alias for the Date property editor.
                /// </summary>
                public const string Date = "Umbraco.Date";

                /// <summary>
                ///     Legacy alias for the Content Picker 2 property editor.
                /// </summary>
                public const string ContentPicker2 = "Umbraco.ContentPicker2";

                /// <summary>
                ///     Legacy alias for the Media Picker 2 property editor.
                /// </summary>
                public const string MediaPicker2 = "Umbraco.MediaPicker2";

                /// <summary>
                ///     Legacy alias for the Member Picker 2 property editor.
                /// </summary>
                public const string MemberPicker2 = "Umbraco.MemberPicker2";

                /// <summary>
                ///     Legacy alias for the Multi Node Tree Picker 2 property editor.
                /// </summary>
                public const string MultiNodeTreePicker2 = "Umbraco.MultiNodeTreePicker2";

                /// <summary>
                ///     Legacy alias for the Textbox Multiple property editor.
                /// </summary>
                public const string TextboxMultiple = "Umbraco.TextboxMultiple";

                /// <summary>
                ///     Legacy alias for the Related Links 2 property editor.
                /// </summary>
                public const string RelatedLinks2 = "Umbraco.RelatedLinks2";

                /// <summary>
                ///     Legacy alias for the Related Links property editor.
                /// </summary>
                public const string RelatedLinks = "Umbraco.RelatedLinks";
            }
        }

        /// <summary>
        ///     Defines Umbraco built-in property editor aliases.
        /// </summary>
        public static class Aliases
        {
            /// <summary>
            ///     Block List.
            /// </summary>
            public const string BlockList = "Umbraco.BlockList";

            /// <summary>
            ///     Block List.
            /// </summary>
            public const string SingleBlock = "Umbraco.SingleBlock";

            /// <summary>
            /// Block Grid.
            /// </summary>
            public const string BlockGrid = "Umbraco.BlockGrid";

                /// <summary>
            ///     CheckBox List.
            /// </summary>
            public const string CheckBoxList = "Umbraco.CheckBoxList";

            /// <summary>
            ///     Color Picker.
            /// </summary>
            public const string ColorPicker = "Umbraco.ColorPicker";

            /// <summary>
            ///     Eye Dropper Color Picker.
            /// </summary>
            public const string ColorPickerEyeDropper = "Umbraco.ColorPicker.EyeDropper";

            /// <summary>
            ///     Content Picker.
            /// </summary>
            public const string ContentPicker = "Umbraco.ContentPicker";

            /// <summary>
            ///     DateTime.
            /// </summary>
            public const string DateTime = "Umbraco.DateTime";

            /// <summary>
            ///     Date Time (unspecified).
            /// </summary>
            public const string DateTimeUnspecified = "Umbraco.DateTimeUnspecified";

            /// <summary>
            ///     Date Time (with time zone).
            /// </summary>
            public const string DateTimeWithTimeZone = "Umbraco.DateTimeWithTimeZone";

            /// <summary>
            ///     Date Only.
            /// </summary>
            public const string DateOnly = "Umbraco.DateOnly";

            /// <summary>
            ///     Entity Data Picker
            /// </summary>
            public const string EntityDataPicker = "Umbraco.EntityDataPicker";

            /// <summary>
            ///     Time Only.
            /// </summary>
            public const string TimeOnly = "Umbraco.TimeOnly";

            /// <summary>
            ///     DropDown List, holding any number of values.
            /// </summary>
            /// <remarks>
            ///     The alias reads "flexible" because the editor used to be flexible about how many values it held.
            ///     It no longer is - a dropdown holding a single value is <see cref="SingleDropDown" /> - but the
            ///     alias is stored against every dropdown data type, so it stays as it is.
            /// </remarks>
            public const string MultipleDropDown = "Umbraco.DropDown.Flexible";

            /// <summary>
            ///     DropDown List, holding a single value.
            /// </summary>
            public const string SingleDropDown = "Umbraco.SingleDropDown";

            /// <summary>
            ///     DropDown List.
            /// </summary>
            [Obsolete("Use MultipleDropDown instead, which names the same editor for what it now does. Scheduled for removal in Umbraco 21.")]
            public const string DropDownListFlexible = MultipleDropDown;

            /// <summary>
            ///     Grid.
            /// </summary>
            public const string Grid = "Umbraco.Grid";

            /// <summary>
            ///     Image Cropper.
            /// </summary>
            public const string ImageCropper = "Umbraco.ImageCropper";

            /// <summary>
            ///     Integer.
            /// </summary>
            public const string Integer = "Umbraco.Integer";

            /// <summary>
            ///     Decimal.
            /// </summary>
            public const string Decimal = "Umbraco.Decimal";

            /// <summary>
            ///     ListView.
            /// </summary>
            public const string ListView = "Umbraco.ListView";

            /// <summary>
            ///     Media Picker v.3.
            /// </summary>
            public const string MediaPicker3 = "Umbraco.MediaPicker3";

            /// <summary>
            ///     Media Picker, holding a single item.
            /// </summary>
            public const string SingleMediaPicker = "Umbraco.SingleMediaPicker";

            /// <summary>
            ///     Multiple Media Picker.
            /// </summary>
            public const string MultipleMediaPicker = "Umbraco.MultipleMediaPicker";

            /// <summary>
            ///     Member Picker.
            /// </summary>
            public const string MemberPicker = "Umbraco.MemberPicker";

            /// <summary>
            ///     Member Group Picker.
            /// </summary>
            public const string MemberGroupPicker = "Umbraco.MemberGroupPicker";

            /// <summary>
            ///     MultiNode Tree Picker.
            /// </summary>
            public const string MultiNodeTreePicker = "Umbraco.MultiNodeTreePicker";

            /// <summary>
            ///     Multiple TextString.
            /// </summary>
            public const string MultipleTextstring = "Umbraco.MultipleTextstring";

            /// <summary>
            ///     Label, holding a string.
            /// </summary>
            public const string Label = "Umbraco.Label";

            /// <summary>
            ///     Label, holding a long string.
            /// </summary>
            public const string LabelText = "Umbraco.Label.Text";

            /// <summary>
            ///     Label, holding an integer.
            /// </summary>
            public const string LabelInteger = "Umbraco.Label.Integer";

            /// <summary>
            ///     Label, holding a big integer.
            /// </summary>
            public const string LabelBigInt = "Umbraco.Label.BigInt";

            /// <summary>
            ///     Label, holding a decimal.
            /// </summary>
            public const string LabelDecimal = "Umbraco.Label.Decimal";

            /// <summary>
            ///     Label, holding a date and time.
            /// </summary>
            public const string LabelDateTime = "Umbraco.Label.DateTime";

            /// <summary>
            ///     Label, holding a time.
            /// </summary>
            public const string LabelTime = "Umbraco.Label.Time";

            /// <summary>
            ///     Picker Relations.
            /// </summary>
            public const string PickerRelations = "Umbraco.PickerRelations";

            /// <summary>
            ///     RadioButton list.
            /// </summary>
            public const string RadioButtonList = "Umbraco.RadioButtonList";

            /// <summary>
            ///     Slider, holding a single value.
            /// </summary>
            public const string Slider = "Umbraco.Slider";

            /// <summary>
            ///     Slider, holding a range of two values.
            /// </summary>
            public const string RangeSlider = "Umbraco.RangeSlider";

            /// <summary>
            ///     Tags.
            /// </summary>
            public const string Tags = "Umbraco.Tags";

            /// <summary>
            ///     Textbox.
            /// </summary>
            public const string TextBox = "Umbraco.TextBox";

            /// <summary>
            ///     Textbox Multiple.
            /// </summary>
            public const string TextArea = "Umbraco.TextArea";

            /// <summary>
            ///     Rich Text Editor.
            /// </summary>
            public const string RichText = "Umbraco.RichText";

            /// <summary>
            ///     Boolean.
            /// </summary>
            public const string Boolean = "Umbraco.TrueFalse";

            /// <summary>
            ///     Markdown Editor.
            /// </summary>
            public const string MarkdownEditor = "Umbraco.MarkdownEditor";

            /// <summary>
            ///     User Picker.
            /// </summary>
            public const string UserPicker = "Umbraco.UserPicker";

            /// <summary>
            ///     Upload Field.
            /// </summary>
            public const string UploadField = "Umbraco.UploadField";

            /// <summary>
            ///     Email Address.
            /// </summary>
            public const string EmailAddress = "Umbraco.EmailAddress";

            /// <summary>
            ///     Nested Content.
            /// </summary>
            public const string NestedContent = "Umbraco.NestedContent";

            /// <summary>
            ///     Alias for the multi URL picker editor.
            /// </summary>
            public const string MultiUrlPicker = "Umbraco.MultiUrlPicker";

            /// <summary>
            ///     Alias for the URL picker editor holding a single link.
            /// </summary>
            public const string SingleUrlPicker = "Umbraco.SingleUrlPicker";

            /// <summary>
            ///     Configuration-less string.
            /// </summary>
            public const string PlainString = "Umbraco.Plain.String";

            /// <summary>
            ///     Configuration-less JSON.
            /// </summary>
            public const string PlainJson = "Umbraco.Plain.Json";

            /// <summary>
            ///     Configuration-less decimal.
            /// </summary>
            public const string PlainDecimal = "Umbraco.Plain.Decimal";

            /// <summary>
            ///     Configuration-less integer.
            /// </summary>
            public const string PlainInteger = "Umbraco.Plain.Integer";

            /// <summary>
            ///     Configuration-less date/time.
            /// </summary>
            public const string PlainDateTime = "Umbraco.Plain.DateTime";

            /// <summary>
            ///     Configuration-less time.
            /// </summary>
            public const string PlainTime = "Umbraco.Plain.Time";

            /// <summary>
            ///     Element Picker.
            /// </summary>
            public const string ElementPicker = "Umbraco.ElementPicker";
        }

        /// <summary>
        ///     Defines Umbraco build-in datatype configuration keys.
        /// </summary>
        public static class ConfigurationKeys
        {
            /// <summary>
            ///     The value type of property data (i.e., string, integer, etc)
            /// </summary>
            /// <remarks>Must be a valid <see cref="ValueTypes" /> value.</remarks>
            public const string DataValueType = "umbracoDataValueType";
        }

        /// <summary>
        ///     Defines Umbraco's built-in property editor groups.
        /// </summary>
        public static class Groups
        {
            /// <summary>
            ///     The Common property editor group.
            /// </summary>
            public const string Common = "Common";

            /// <summary>
            ///     The Lists property editor group.
            /// </summary>
            public const string Lists = "Lists";

            /// <summary>
            ///     The Media property editor group.
            /// </summary>
            public const string Media = "Media";

            /// <summary>
            ///     The People property editor group.
            /// </summary>
            public const string People = "People";

            /// <summary>
            ///     The Pickers property editor group.
            /// </summary>
            public const string Pickers = "Pickers";

            /// <summary>
            ///     The Rich Content property editor group.
            /// </summary>
            public const string RichContent = "Rich Content";
        }
    }
}
