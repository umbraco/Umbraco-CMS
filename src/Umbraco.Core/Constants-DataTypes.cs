using System;

namespace Umbraco.Core
{
    public static partial class Constants
    {
        /// <summary>
        ///     Defines the identifiers for Umbraco data types as constants for easy centralized access/management.
        /// </summary>
        public static class DataTypes
        {

            public static class ReservedPreValueKeys
            {
                public const string IgnoreUserStartNodes = "ignoreUserStartNodes";
            }

            /// <summary>
            /// Guid for Content Picker as string
            /// </summary>
            public const string ContentPicker = "FD1E0DA5-5606-4862-B679-5D0CF3A52A59";

            /// <summary>
            /// Guid for Content Picker
            /// </summary>
            public static readonly Guid ContentPickerGuid = new Guid(ContentPicker);


            /// <summary>
            /// Guid for Member Picker as string
            /// </summary>
            public const string MemberPicker = "1EA2E01F-EBD8-4CE1-8D71-6B1149E63548";

            /// <summary>
            /// Guid for Member Picker
            /// </summary>
            public static readonly Guid MemberPickerGuid = new Guid(MemberPicker);


            /// <summary>
            /// Guid for Media Picker as string
            /// </summary>
            public const string MediaPicker = "135D60E0-64D9-49ED-AB08-893C9BA44AE5";

            /// <summary>
            /// Guid for Media Picker
            /// </summary>
            public static readonly Guid MediaPickerGuid = new Guid(MediaPicker);


            /// <summary>
            /// Guid for Multiple Media Picker as string
            /// </summary>
            public const string MultipleMediaPicker = "9DBBCBBB-2327-434A-B355-AF1B84E5010A";

            /// <summary>
            /// Guid for Multiple Media Picker
            /// </summary>
            public static readonly Guid MultipleMediaPickerGuid = new Guid(MultipleMediaPicker);


            /// <summary>
            /// Guid for Related Links as string
            /// </summary>
            public const string RelatedLinks = "B4E3535A-1753-47E2-8568-602CF8CFEE6F";

            /// <summary>
            /// Guid for Related Links
            /// </summary>
            public static readonly Guid RelatedLinksGuid = new Guid(RelatedLinks);


            /// <summary>
            /// Guid for Member as string
            /// </summary>
            public const string Member = "d59be02f-1df9-4228-aa1e-01917d806cda";

            /// <summary>
            /// Guid for Member
            /// </summary>
            public static readonly Guid MemberGuid = new Guid(Member);


            /// <summary>
            /// Guid for Image Cropper as string
            /// </summary>
            public const string ImageCropper = "1df9f033-e6d4-451f-b8d2-e0cbc50a836f";

            /// <summary>
            /// Guid for Image Cropper
            /// </summary>
            public static readonly Guid ImageCropperGuid = new Guid(ImageCropper);


            /// <summary>
            /// Guid for Tags as string
            /// </summary>
            public const string Tags = "b6b73142-b9c1-4bf8-a16d-e1c23320b549";

            /// <summary>
            /// Guid for Tags
            /// </summary>
            public static readonly Guid TagsGuid = new Guid(Tags);


            /// <summary>
            /// Guid for List View - Content as string
            /// </summary>
            public const string ListViewContent = "C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4";

            /// <summary>
            /// Guid for List View - Content
            /// </summary>
            public static readonly Guid ListViewContentGuid = new Guid(ListViewContent);


            /// <summary>
            /// Guid for List View - Media as string
            /// </summary>
            public const string ListViewMedia = "3A0156C4-3B8C-4803-BDC1-6871FAA83FFF";

            /// <summary>
            /// Guid for List View - Media
            /// </summary>
            public static readonly Guid ListViewMediaGuid = new Guid(ListViewMedia);


            /// <summary>
            /// Guid for List View - Members as string
            /// </summary>
            public const string ListViewMembers = "AA2C52A0-CE87-4E65-A47C-7DF09358585D";

            /// <summary>
            /// Guid for List View - Members
            /// </summary>
            public static readonly Guid ListViewMembersGuid = new Guid(ListViewMembers);


            /// <summary>
            /// Guid for Date Picker with time as string
            /// </summary>
            public const string DatePickerWithTime = "e4d66c0f-b935-4200-81f0-025f7256b89a";

            /// <summary>
            /// Guid for Date Picker with time
            /// </summary>
            public static readonly Guid DatePickerWithTimeGuid = new Guid(DatePickerWithTime);


            /// <summary>
            /// Guid for Approved Color as string
            /// </summary>
            public const string ApprovedColor = "0225af17-b302-49cb-9176-b9f35cab9c17";

            /// <summary>
            /// Guid for Approved Color
            /// </summary>
            public static readonly Guid ApprovedColorGuid = new Guid(ApprovedColor);


            /// <summary>
            /// Guid for Dropdown multiple as string
            /// </summary>
            public const string DropdownMultiple = "f38f0ac7-1d27-439c-9f3f-089cd8825a53";

            /// <summary>
            /// Guid for Dropdown multiple
            /// </summary>
            public static readonly Guid DropdownMultipleGuid = new Guid(DropdownMultiple);


            /// <summary>
            /// Guid for Radiobox as string
            /// </summary>
            public const string Radiobox = "bb5f57c9-ce2b-4bb9-b697-4caca783a805";

            /// <summary>
            /// Guid for Radiobox
            /// </summary>
            public static readonly Guid RadioboxGuid = new Guid(Radiobox);


            /// <summary>
            /// Guid for Date Picker as string
            /// </summary>
            public const string DatePicker = "5046194e-4237-453c-a547-15db3a07c4e1";

            /// <summary>
            /// Guid for Date Picker
            /// </summary>
            public static readonly Guid DatePickerGuid = new Guid(DatePicker);


            /// <summary>
            /// Guid for Dropdown as string
            /// </summary>
            public const string Dropdown = "0b6a45e7-44ba-430d-9da5-4e46060b9e03";

            /// <summary>
            /// Guid for Dropdown
            /// </summary>
            public static readonly Guid DropdownGuid = new Guid(Dropdown);


            /// <summary>
            /// Guid for Checkbox list as string
            /// </summary>
            public const string CheckboxList = "fbaf13a8-4036-41f2-93a3-974f678c312a";

            /// <summary>
            /// Guid for Checkbox list
            /// </summary>
            public static readonly Guid CheckboxListGuid = new Guid(CheckboxList);


            /// <summary>
            /// Guid for Checkbox as string
            /// </summary>
            public const string Checkbox = "92897bc6-a5f3-4ffe-ae27-f2e7e33dda49";

            /// <summary>
            /// Guid for Checkbox
            /// </summary>
            public static readonly Guid CheckboxGuid = new Guid(Checkbox);


            /// <summary>
            /// Guid for Numeric as string
            /// </summary>
            public const string Numeric = "2e6d3631-066e-44b8-aec4-96f09099b2b5";

            /// <summary>
            /// Guid for Dropdown
            /// </summary>
            public static readonly Guid NumericGuid = new Guid(Numeric);


            /// <summary>
            /// Guid for Richtext editor as string
            /// </summary>
            public const string RichtextEditor = "ca90c950-0aff-4e72-b976-a30b1ac57dad";

            /// <summary>
            /// Guid for Richtext editor
            /// </summary>
            public static readonly Guid RichtextEditorGuid = new Guid(RichtextEditor);


            /// <summary>
            /// Guid for Textstring as string
            /// </summary>
            public const string Textstring = "0cc0eba1-9960-42c9-bf9b-60e150b429ae";

            /// <summary>
            /// Guid for Textstring
            /// </summary>
            public static readonly Guid TextstringGuid = new Guid(Textstring);


            /// <summary>
            /// Guid for Textarea as string
            /// </summary>
            public const string Textarea = "c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3";

            /// <summary>
            /// Guid for Dropdown
            /// </summary>
            public static readonly Guid TextareaGuid = new Guid(Textarea);


            /// <summary>
            /// Guid for Upload as string
            /// </summary>
            public const string Upload = "84c6b441-31df-4ffe-b67e-67d5bc3ae65a";

            /// <summary>
            /// Guid for Upload
            /// </summary>
            public static readonly Guid UploadGuid = new Guid(Upload);


            /// <summary>
            /// Guid for Label as string
            /// </summary>
            public const string Label = "f0bc4bfb-b499-40d6-ba86-058885a5178c";

            /// <summary>
            /// Guid for Label
            /// </summary>
            public static readonly Guid LabelGuid = new Guid(Label);


        }
    }
}
