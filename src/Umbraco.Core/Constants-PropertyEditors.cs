using System;

namespace Umbraco.Core
{
	public static partial class Constants
    {
        /// <summary>
        /// Defines the identifiers for Umbraco Property Editors as constants for easy centralized access/management.
        /// </summary>
        public static class PropertyEditors
        {
            /// <summary>
            /// Used to prefix generic properties that are internal content properties
            /// </summary>            
            public const string InternalGenericPropertiesPrefix = "_umb_";

            /// <summary>
            /// Guid for the Checkbox list datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string CheckBoxList = "B4471851-82B6-4C75-AFA4-39FA9C6A75E9";

            /// <summary>
            /// Alias for Checkbox list datatype.
            /// </summary>
            public const string CheckBoxListAlias = "Umbraco.CheckBoxList";

            /// <summary>
            /// Guid for the Color Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string ColorPicker = "F8D60F68-EC59-4974-B43B-C46EB5677985";

            /// <summary>
            /// Alias for the Color Picker datatype.
            /// </summary>
            public const string ColorPickerAlias = "Umbraco.ColorPickerAlias";

            /// <summary>
            /// Guid for the Content Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string ContentPicker = "158AA029-24ED-4948-939E-C3DA209E5FBA";

            /// <summary>
            /// Alias for the Content Picker datatype.
            /// </summary>
            public const string ContentPickerAlias = "Umbraco.ContentPickerAlias";

            /// <summary>
            /// Guid for the Date datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string Date = "23E93522-3200-44E2-9F29-E61A6FCBB79A";

            /// <summary>
            /// Alias for the Date datatype.
            /// </summary>
            public const string DateAlias = "Umbraco.Date";

            /// <summary>
            /// Guid for the Date/Time datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DateTime = "B6FB1622-AFA5-4BBF-A3CC-D9672A442222";

            /// <summary>
            /// Alias for the Date/Time datatype.
            /// </summary>
            public const string DateTimeAlias = "Umbraco.DateTime";

            /// <summary>
            /// Guid for the Dictionary Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DictionaryPicker = "17B70066-F764-407D-AB05-3717F1E1C513";
            
            /// <summary>
            /// Guid for the Dropdown list datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DropDownList = "A74EA9C9-8E18-4D2A-8CF6-73C6206C5DA6";

            /// <summary>
            /// Alias for the Dropdown list datatype.
            /// </summary>
            public const string DropDownListAlias = "Umbraco.DropDown";

            /// <summary>
            /// Guid for the Dropdown list multiple datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DropDownListMultiple = "928639ED-9C73-4028-920C-1E55DBB68783";

            /// <summary>
            /// Alias for the Dropdown list multiple datatype.
            /// </summary>
            public const string DropDownListMultipleAlias = "Umbraco.DropDownMultiple";

            /// <summary>
            /// Guid for the Dropdown list multiple, publish keys datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DropdownlistMultiplePublishKeys = "928639AA-9C73-4028-920C-1E55DBB68783";

            /// <summary>
            /// Alias for the Dropdown list multiple, publish keys datatype.
            /// </summary>
            public const string DropdownlistMultiplePublishKeysAlias = "Umbraco.DropdownlistMultiplePublishKeys";

            /// <summary>
            /// Guid for the Dropdown list, publishing keys datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string DropdownlistPublishingKeys = "A74EA9E1-8E18-4D2A-8CF6-73C6206C5DA6";

            /// <summary>
            /// Alias for the Dropdown list, publishing keys datatype.
            /// </summary>
            public const string DropdownlistPublishingKeysAlias = "Umbraco.DropdownlistPublishingKeys";

            /// <summary>
            /// Guid for the Folder browser datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string FolderBrowser = "CCCD4AE9-F399-4ED2-8038-2E88D19E810C";

            /// <summary>
            /// Alias for the Folder browser datatype.
            /// </summary>
            public const string FolderBrowserAlias = "Umbraco.FolderBrowser";

            /// <summary>
            /// Alias for the grid datatype.
            /// </summary>
            public const string GridAlias = "Umbraco.Grid";


            /// <summary>
            /// Guid for the Image Cropper datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string ImageCropper = "7A2D436C-34C2-410F-898F-4A23B3D79F54";

            ///// <summary>
            ///// Alias for the Image Cropper datatype.
            ///// </summary>
            public const string ImageCropperAlias = "Umbraco.ImageCropper";

            /// <summary>
            /// Guid for the Integer datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string Integer = "1413AFCB-D19A-4173-8E9A-68288D2A73B8";

            /// <summary>
            /// Alias for the Integer datatype.
            /// </summary>
            public const string IntegerAlias = "Umbraco.Integer";

            /// <summary>
            /// Alias for the Decimal datatype.
            /// </summary>
            public const string DecimalAlias = "Umbraco.Decimal";

            /// <summary>
            /// Alias for the listview datatype.
            /// </summary>
            public const string ListViewAlias = "Umbraco.ListView";

            /// <summary>
            /// Guid for the list view datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string ListView = "474FCFF8-9D2D-12DE-ABC6-AD7A56D89593";


            /// <summary>
            /// Guid for the Macro Container datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string MacroContainer = "474FCFF8-9D2D-11DE-ABC6-AD7A56D89593";

            /// <summary>
            /// Alias for the Macro Container datatype.
            /// </summary>
            public const string MacroContainerAlias = "Umbraco.MacroContainer";

            /// <summary>
            /// Guid for the Media Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string MediaPicker = "EAD69342-F06D-4253-83AC-28000225583B";

            /// <summary>
            /// Alias for the Media Picker datatype.
            /// </summary>
            public const string MediaPickerAlias = "Umbraco.MediaPicker";

            public const string MultipleMediaPickerAlias = "Umbraco.MultipleMediaPicker";

            /// <summary>
            /// Guid for the Member Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string MemberPicker = "39F533E4-0551-4505-A64B-E0425C5CE775";

            /// <summary>
            /// Alias for the Member Picker datatype.
            /// </summary>
            public const string MemberPickerAlias = "Umbraco.MemberPicker";

            /// <summary>
            /// Alias for the Member Group Picker datatype.
            /// </summary>
            public const string MemberGroupPickerAlias = "Umbraco.MemberGroupPicker";

            /// <summary>
            /// Guid for the Multi-Node Tree Picker datatype
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string MultiNodeTreePicker = "7E062C13-7C41-4AD9-B389-41D88AEEF87C";

            /// <summary>
            /// Alias for the Multi-Node Tree Picker datatype
            /// </summary>
            public const string MultiNodeTreePickerAlias = "Umbraco.MultiNodeTreePicker";

            /// <summary>
            /// Guid for the Multiple Textstring datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string MultipleTextstring = "5359AD0B-06CC-4182-92BD-0A9117448D3F";

            /// <summary>
            /// Alias for the Multiple Textstring datatype.
            /// </summary>
            public const string MultipleTextstringAlias = "Umbraco.MultipleTextstring";

            /// <summary>
            /// Guid for the No edit datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string NoEdit = "6C738306-4C17-4D88-B9BD-6546F3771597";

            /// <summary>
            /// Alias for the No edit datatype.
            /// </summary>
            public const string NoEditAlias = "Umbraco.NoEdit";

            /// <summary>
            /// Guid for the Picker Relations datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string PickerRelations = "83396FF2-2E39-4A90-9066-17F5F3989374";

            /// <summary>
            /// Alias for the Picker Relations datatype.
            /// </summary>
            public const string PickerRelationsAlias = "Umbraco.PickerRelations";

            /// <summary>
            /// Guid for the Radiobutton list datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string RadioButtonList = "A52C7C1C-C330-476E-8605-D63D3B84B6A6";

            /// <summary>
            /// Alias for the Radiobutton list datatype.
            /// </summary>
            public const string RadioButtonListAlias = "Umbraco.RadioButtonList";

            /// <summary>
            /// Guid for the Related Links datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string RelatedLinks = "71B8AD1A-8DC2-425C-B6B8-FAA158075E63";

            /// <summary>
            /// Alias for the Related Links datatype.
            /// </summary>
            public const string RelatedLinksAlias = "Umbraco.RelatedLinks";

            /// <summary>
            /// Guid for the Slider datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string Slider = "29E790E6-26B3-438A-B21F-908663A0B19E";

            /// <summary>
            /// Alias for the Slider datatype.
            /// </summary>
            public const string SliderAlias = "Umbraco.Slider";

            /// <summary>
            /// Guid for the Tags datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string Tags = "4023E540-92F5-11DD-AD8B-0800200C9A66";

            /// <summary>
            /// Alias for the Tags datatype.
            /// </summary>
            public const string TagsAlias = "Umbraco.Tags";

            /// <summary>
            /// Guid for the Textbox datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string Textbox = "EC15C1E5-9D90-422A-AA52-4F7622C63BEA";

            /// <summary>
            /// Alias for the Textbox datatype.
            /// </summary>
            public const string TextboxAlias = "Umbraco.Textbox";

            /// <summary>
            /// Guid for the Textarea datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string TextboxMultiple = "67DB8357-EF57-493E-91AC-936D305E0F2A";

            /// <summary>
            /// Alias for the Textarea datatype.
            /// </summary>
            public const string TextboxMultipleAlias = "Umbraco.TextboxMultiple";

            /// <summary>
            /// Guid for the TinyMCE v3 wysiwyg datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string TinyMCEv3 = "5E9B75AE-FACE-41C8-B47E-5F4B0FD82F83";

            /// <summary>
            /// Alias for the TinyMCE wysiwyg datatype.
            /// </summary>
            public const string TinyMCEAlias = "Umbraco.TinyMCEv3";

            /// <summary>
            /// Guid for the True/False (Ja/Nej) datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string TrueFalse = "38B352C1-E9F8-4FD8-9324-9A2EAB06D97A";

            /// <summary>
            /// Alias for the True/False (Ja/Nej) datatype.
            /// </summary>
            public const string TrueFalseAlias = "Umbraco.TrueFalse";

            /// <summary>
            /// Guid for the Ultimate Picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string UltimatePicker = "CDBF0B5D-5CB2-445F-BC12-FCAAEC07CF2C";
            
            /// <summary>
            /// Guid for the UltraSimpleEditor datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string UltraSimpleEditor = "60B7DABF-99CD-41EB-B8E9-4D2E669BBDE9";

            /// <summary>
            /// Alias for the MarkdownEditor datatype.
            /// </summary>
            public const string MarkdownEditorAlias = "Umbraco.MarkdownEditor";

            /// <summary>
            /// Guid for the Umbraco Usercontrol Wrapper datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string UmbracoUserControlWrapper = "D15E1281-E456-4B24-AA86-1DDA3E4299D5";
            
            /// <summary>
            /// Guid for the Upload field datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string UploadField = "5032A6E6-69E3-491D-BB28-CD31CD11086C";

            /// <summary>
            /// Alias for the User picker datatype.
            /// </summary>
            public const string UserPickerAlias = "Umbraco.UserPicker";

            /// <summary>
            /// Guid for the User picker datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string UserPicker = "e66af4a0-e8b4-11de-8a39-0800200c9a66";

            /// <summary>
            /// Alias for the Upload field datatype.
            /// </summary>
            public const string UploadFieldAlias = "Umbraco.UploadField";


            /// <summary>
            /// Guid for the XPath CheckBoxList datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string XPathCheckBoxList = "34451D92-D270-49BA-8C7F-EE55BFEEE1CB";

            /// <summary>
            /// Alias for the XPath CheckBoxList datatype.
            /// </summary>
            public const string XPathCheckBoxListAlias = "Umbraco.XPathCheckBoxList";

            /// <summary>
            /// Guid for the XPath DropDownList datatype.
            /// </summary>
            [Obsolete("GUIDs are no longer used to reference Property Editors, use the Alias constant instead. This will be removed in future versions")]
            public const string XPathDropDownList = "173A96AE-00ED-4A7C-9F76-4B53D4A0A1B9";

            /// <summary>
            /// Alias for the XPath DropDownList datatype.
            /// </summary>
            public const string XPathDropDownListAlias = "Umbraco.XPathDropDownList";

            /// <summary>
            /// Alias for the email address property editor
            /// </summary>
            public const string EmailAddressAlias = "Umbraco.EmailAddress";
        }
	}
}