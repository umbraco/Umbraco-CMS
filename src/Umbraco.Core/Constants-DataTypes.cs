namespace Umbraco.Cms.Core;

public static partial class Constants
{
    public static class DataTypes
    {
        // NOTE: unfortunately due to backwards compat we can't move/rename these, with the addition of the GUID
        // constants, it would make more sense to have these suffixed with "ID" or in a Subclass called "INT", for
        // now all we can do is make a subclass called Guids to put the GUID IDs.
        public const int LabelString = System.DefaultLabelDataTypeId;
        public const int LabelInt = -91;
        public const int LabelBigint = -93;
        public const int LabelDateTime = -94;
        public const int LabelTime = -98;
        public const int LabelDecimal = -99;
        public const int LabelBytes = -104;
        public const int LabelPixels = -105;

        public const int Textarea = -89;
        public const int Textbox = -88;
        public const int RichtextEditor = -87;
        public const int Boolean = -49;
        public const int DateTime = -36;
        public const int DropDownSingle = -39;
        public const int DropDownMultiple = -42;
        public const int Upload = -90;
        public const int UploadVideo = -100;
        public const int UploadAudio = -101;
        public const int UploadArticle = -102;
        public const int UploadVectorGraphics = -103;

        public const int DefaultContentListView = -95;
        public const int DefaultMediaListView = -96;
        public const int DefaultMembersListView = -97;

        public const int ImageCropper = 1043;
        public const int Tags = 1041;

        public static class ReservedPreValueKeys
        {
            public const string IgnoreUserStartNodes = "ignoreUserStartNodes";
        }

        /// <summary>
        ///     Defines the identifiers for Umbraco data types as constants for easy centralized access/management.
        /// </summary>
        public static class Guids
        {
            /// <summary>
            ///     Guid for Content Picker as string
            /// </summary>
            public const string ContentPicker = "FD1E0DA5-5606-4862-B679-5D0CF3A52A59";

            /// <summary>
            ///     Guid for Member Picker as string
            /// </summary>
            public const string MemberPicker = "1EA2E01F-EBD8-4CE1-8D71-6B1149E63548";

            /// <summary>
            ///     Guid for Media Picker v3 as string
            /// </summary>
            public const string MediaPicker3 = "4309A3EA-0D78-4329-A06C-C80B036AF19A";

            /// <summary>
            ///     Guid for Media Picker v3 multiple as string
            /// </summary>
            public const string MediaPicker3Multiple = "1B661F40-2242-4B44-B9CB-3990EE2B13C0";

            /// <summary>
            ///     Guid for Media Picker v3 single-image as string
            /// </summary>
            public const string MediaPicker3SingleImage = "AD9F0CF2-BDA2-45D5-9EA1-A63CFC873FD3";

            /// <summary>
            ///     Guid for Media Picker v3 multi-image as string
            /// </summary>
            public const string MediaPicker3MultipleImages = "0E63D883-B62B-4799-88C3-157F82E83ECC";

            /// <summary>
            ///     Guid for Related Links as string
            /// </summary>
            public const string RelatedLinks = "B4E3535A-1753-47E2-8568-602CF8CFEE6F";

            /// <summary>
            ///     Guid for Member as string
            /// </summary>
            public const string Member = "d59be02f-1df9-4228-aa1e-01917d806cda";

            /// <summary>
            ///     Guid for Image Cropper as string
            /// </summary>
            public const string ImageCropper = "1df9f033-e6d4-451f-b8d2-e0cbc50a836f";

            /// <summary>
            ///     Guid for Tags as string
            /// </summary>
            public const string Tags = "b6b73142-b9c1-4bf8-a16d-e1c23320b549";

            /// <summary>
            ///     Guid for List View - Content as string
            /// </summary>
            public const string ListViewContent = "C0808DD3-8133-4E4B-8CE8-E2BEA84A96A4";

            /// <summary>
            ///     Guid for List View - Media as string
            /// </summary>
            public const string ListViewMedia = "3A0156C4-3B8C-4803-BDC1-6871FAA83FFF";

            /// <summary>
            ///     Guid for List View - Members as string
            /// </summary>
            [Obsolete("No longer used in Umbraco. Scheduled for removal in Umbraco 17.")]
            public const string ListViewMembers = "AA2C52A0-CE87-4E65-A47C-7DF09358585D";

            /// <summary>
            ///     Guid for Date Picker with time as string
            /// </summary>
            public const string DatePickerWithTime = "e4d66c0f-b935-4200-81f0-025f7256b89a";

            /// <summary>
            ///     Guid for Approved Color as string
            /// </summary>
            public const string ApprovedColor = "0225af17-b302-49cb-9176-b9f35cab9c17";

            /// <summary>
            ///     Guid for Dropdown multiple as string
            /// </summary>
            public const string DropdownMultiple = "f38f0ac7-1d27-439c-9f3f-089cd8825a53";

            /// <summary>
            ///     Guid for Radiobox as string
            /// </summary>
            public const string Radiobox = "bb5f57c9-ce2b-4bb9-b697-4caca783a805";

            /// <summary>
            ///     Guid for Date Picker as string
            /// </summary>
            public const string DatePicker = "5046194e-4237-453c-a547-15db3a07c4e1";

            /// <summary>
            ///     Guid for Dropdown as string
            /// </summary>
            public const string Dropdown = "0b6a45e7-44ba-430d-9da5-4e46060b9e03";

            /// <summary>
            ///     Guid for Checkbox list as string
            /// </summary>
            public const string CheckboxList = "fbaf13a8-4036-41f2-93a3-974f678c312a";

            /// <summary>
            ///     Guid for Checkbox as string
            /// </summary>
            public const string Checkbox = "92897bc6-a5f3-4ffe-ae27-f2e7e33dda49";

            /// <summary>
            ///     Guid for Numeric as string
            /// </summary>
            public const string Numeric = "2e6d3631-066e-44b8-aec4-96f09099b2b5";

            /// <summary>
            ///     Guid for Richtext editor as string
            /// </summary>
            public const string RichtextEditor = "ca90c950-0aff-4e72-b976-a30b1ac57dad";

            /// <summary>
            ///     Guid for Textstring as string
            /// </summary>
            public const string Textstring = "0cc0eba1-9960-42c9-bf9b-60e150b429ae";

            /// <summary>
            ///     Guid for Textarea as string
            /// </summary>
            public const string Textarea = "c6bac0dd-4ab9-45b1-8e30-e4b619ee5da3";

            /// <summary>
            ///     Guid for Upload as string
            /// </summary>
            public const string Upload = "84c6b441-31df-4ffe-b67e-67d5bc3ae65a";

            /// <summary>
            ///     Guid for UploadVideo as string
            /// </summary>
            public const string UploadVideo = "70575fe7-9812-4396-bbe1-c81a76db71b5";

            /// <summary>
            ///     Guid for UploadAudio as string
            /// </summary>
            public const string UploadAudio = "8f430dd6-4e96-447e-9dc0-cb552c8cd1f3";

            /// <summary>
            ///     Guid for UploadArticle as string
            /// </summary>
            public const string UploadArticle = "bc1e266c-dac4-4164-bf08-8a1ec6a7143d";

            /// <summary>
            ///     Guid for UploadVectorGraphics as string
            /// </summary>
            public const string UploadVectorGraphics = "215cb418-2153-4429-9aef-8c0f0041191b";

            /// <summary>
            ///     Guid for Label as string
            /// </summary>
            public const string LabelString = "f0bc4bfb-b499-40d6-ba86-058885a5178c";

            /// <summary>
            ///     Guid for Label as int
            /// </summary>
            public const string LabelInt = "8e7f995c-bd81-4627-9932-c40e568ec788";

            /// <summary>
            ///     Guid for Label as big int
            /// </summary>
            public const string LabelBigInt = "930861bf-e262-4ead-a704-f99453565708";

            /// <summary>
            ///     Guid for Label as date time
            /// </summary>
            public const string LabelDateTime = "0e9794eb-f9b5-4f20-a788-93acd233a7e4";

            /// <summary>
            ///     Guid for Label as time
            /// </summary>
            public const string LabelTime = "a97cec69-9b71-4c30-8b12-ec398860d7e8";

            /// <summary>
            ///     Guid for Label as decimal
            /// </summary>
            public const string LabelDecimal = "8f1ef1e1-9de4-40d3-a072-6673f631ca64";

            /// <summary>
            ///     Guid for Label as bytes
            /// </summary>
            public const string LabelBytes = "ba5bdbe6-ab3e-46a8-82b3-2c45f10bc47f";

            /// <summary>
            ///     Guid for Label as pixels
            /// </summary>
            public const string LabelPixels = "5eb57825-e15e-4fc7-8e37-fca65cdafbde";

            /// <summary>
            ///     Guid for Content Picker
            /// </summary>
            public static readonly Guid ContentPickerGuid = new(ContentPicker);

            /// <summary>
            ///     Guid for Member Picker
            /// </summary>
            public static readonly Guid MemberPickerGuid = new(MemberPicker);

            /// <summary>
            ///     Guid for Media Picker v3
            /// </summary>
            public static readonly Guid MediaPicker3Guid = new(MediaPicker3);

            /// <summary>
            ///     Guid for Media Picker v3 multiple
            /// </summary>
            public static readonly Guid MediaPicker3MultipleGuid = new(MediaPicker3Multiple);

            /// <summary>
            ///     Guid for Media Picker v3 single-image
            /// </summary>
            public static readonly Guid MediaPicker3SingleImageGuid = new(MediaPicker3SingleImage);

            /// <summary>
            ///     Guid for Media Picker v3 multi-image
            /// </summary>
            public static readonly Guid MediaPicker3MultipleImagesGuid = new(MediaPicker3MultipleImages);

            /// <summary>
            ///     Guid for Related Links
            /// </summary>
            public static readonly Guid RelatedLinksGuid = new(RelatedLinks);

            /// <summary>
            ///     Guid for Member
            /// </summary>
            public static readonly Guid MemberGuid = new(Member);

            /// <summary>
            ///     Guid for Image Cropper
            /// </summary>
            public static readonly Guid ImageCropperGuid = new(ImageCropper);

            /// <summary>
            ///     Guid for Tags
            /// </summary>
            public static readonly Guid TagsGuid = new(Tags);

            /// <summary>
            ///     Guid for List View - Content
            /// </summary>
            public static readonly Guid ListViewContentGuid = new(ListViewContent);

            /// <summary>
            ///     Guid for List View - Media
            /// </summary>
            public static readonly Guid ListViewMediaGuid = new(ListViewMedia);

            /// <summary>
            ///     Guid for Date Picker with time
            /// </summary>
            public static readonly Guid DatePickerWithTimeGuid = new(DatePickerWithTime);

            /// <summary>
            ///     Guid for Approved Color
            /// </summary>
            public static readonly Guid ApprovedColorGuid = new(ApprovedColor);

            /// <summary>
            ///     Guid for Dropdown multiple
            /// </summary>
            public static readonly Guid DropdownMultipleGuid = new(DropdownMultiple);

            /// <summary>
            ///     Guid for Radiobox
            /// </summary>
            public static readonly Guid RadioboxGuid = new(Radiobox);

            /// <summary>
            ///     Guid for Date Picker
            /// </summary>
            public static readonly Guid DatePickerGuid = new(DatePicker);

            /// <summary>
            ///     Guid for Dropdown
            /// </summary>
            public static readonly Guid DropdownGuid = new(Dropdown);

            /// <summary>
            ///     Guid for Checkbox list
            /// </summary>
            public static readonly Guid CheckboxListGuid = new(CheckboxList);

            /// <summary>
            ///     Guid for Checkbox
            /// </summary>
            public static readonly Guid CheckboxGuid = new(Checkbox);

            /// <summary>
            ///     Guid for Dropdown
            /// </summary>
            public static readonly Guid NumericGuid = new(Numeric);

            /// <summary>
            ///     Guid for Richtext editor
            /// </summary>
            public static readonly Guid RichtextEditorGuid = new(RichtextEditor);

            /// <summary>
            ///     Guid for Textstring
            /// </summary>
            public static readonly Guid TextstringGuid = new(Textstring);

            /// <summary>
            ///     Guid for Dropdown
            /// </summary>
            public static readonly Guid TextareaGuid = new(Textarea);

            /// <summary>
            ///     Guid for Upload
            /// </summary>
            public static readonly Guid UploadGuid = new(Upload);

            /// <summary>
            ///     Guid for UploadVideo
            /// </summary>
            public static readonly Guid UploadVideoGuid = new(UploadVideo);

            /// <summary>
            ///     Guid for UploadAudio
            /// </summary>
            public static readonly Guid UploadAudioGuid = new(UploadAudio);

            /// <summary>
            ///     Guid for UploadArticle
            /// </summary>
            public static readonly Guid UploadArticleGuid = new(UploadArticle);

            /// <summary>
            ///     Guid for UploadVectorGraphics
            /// </summary>
            public static readonly Guid UploadVectorGraphicsGuid = new(UploadVectorGraphics);

            /// <summary>
            ///     Guid for Label string
            /// </summary>
            public static readonly Guid LabelStringGuid = new(LabelString);

            /// <summary>
            ///     Guid for Label int
            /// </summary>
            public static readonly Guid LabelIntGuid = new(LabelInt);

            /// <summary>
            ///     Guid for Label big int
            /// </summary>
            public static readonly Guid LabelBigIntGuid = new(LabelBigInt);

            /// <summary>
            ///     Guid for Label date time
            /// </summary>
            public static readonly Guid LabelDateTimeGuid = new(LabelDateTime);

            /// <summary>
            ///     Guid for Label time
            /// </summary>
            public static readonly Guid LabelTimeGuid = new(LabelTime);

            /// <summary>
            ///     Guid for Label decimal
            /// </summary>
            public static readonly Guid LabelDecimalGuid = new(LabelDecimal);
        }
    }
}
