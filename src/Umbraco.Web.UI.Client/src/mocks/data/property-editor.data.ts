import { UmbData } from './data';

export interface PropertyEditor {
	alias: string;
	name: string;
	icon: string;
	editor: PropertyEditorEditor;
}

export interface PropertyEditorEditor {
	view: string;
}

export const data: Array<PropertyEditor> = [
	{
		alias: 'Umbraco.BlockGrid',
		name: 'Block Grid',
		icon: 'umb:thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.BlockList',
		name: 'Block List',
		icon: 'umb:thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.CheckBoxList',
		name: 'Checkbox list',
		icon: 'umb:bulleted-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ColorPicker',
		name: 'Color Picker',
		icon: 'umb:colorpicker',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ContentPicker',
		name: 'Content Picker',
		icon: 'umb:autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.DateTime',
		name: 'Date/Time',
		icon: 'umb:time',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Decimal',
		name: 'Decimal',
		icon: 'umb:autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.DropDown.Flexible',
		name: 'Dropdown',
		icon: 'umb:indent',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.EmailAddress',
		name: 'Email address',
		icon: 'umb:message',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ColorPicker.EyeDropper',
		name: 'Eye Dropper Color Picker',
		icon: 'umb:colorpicker',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.UploadField',
		name: 'File upload',
		icon: 'umb:download-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Grid',
		name: 'Grid layout',
		icon: 'umb:layout',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ImageCropper',
		name: 'Image Cropper',
		icon: 'umb:crop',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Label',
		name: 'Label',
		icon: 'umb:readonly',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ListView',
		name: 'List view',
		icon: 'umb:thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MarkdownEditor',
		name: 'Markdown editor',
		icon: 'umb:code',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MediaPicker3',
		name: 'Media Picker',
		icon: 'umb:picture',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MediaPicker',
		name: 'Media Picker (legacy)',
		icon: 'umb:picture',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MemberGroupPicker',
		name: 'Member Group Picker',
		icon: 'umb:users-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MemberPicker',
		name: 'Member Picker',
		icon: 'umb:user',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultiUrlPicker',
		name: 'Multi URL Picker',
		icon: 'umb:link',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultiNodeTreePicker',
		name: 'Multinode Treepicker',
		icon: 'umb:page-add',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.NestedContent',
		name: 'Nested Content',
		icon: 'umb:thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Integer',
		name: 'Numeric',
		icon: 'umb:autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.RadioButtonList',
		name: 'Radio button list',
		icon: 'umb:target',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultipleTextstring',
		name: 'Repeatable textstrings',
		icon: 'umb:ordered-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TinyMCE',
		name: 'Rich Text Editor',
		icon: 'umb:browser-window',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Slider',
		name: 'Slider',
		icon: 'umb:navigation-horizontal',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Tags',
		name: 'Tags',
		icon: 'umb:tags',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TextArea',
		name: 'Textarea',
		icon: 'umb:application-window-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TextBox',
		name: 'Textbox',
		icon: 'umb:autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TrueFalse',
		name: 'Toggle',
		icon: 'umb:checkbox',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.UserPicker',
		name: 'User Picker',
		icon: 'umb:user',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Custom',
		name: 'Custom Property Editor',
		icon: 'umb:document',
		editor: {
			view: '',
		},
	},
];

// Temp mocked database
class UmbPropertyEditorData extends UmbData<PropertyEditor> {
	constructor() {
		super(data);
	}

	getAll() {
		return this.data;
	}

	getByAlias(alias: string) {
		return this.data.find((x) => x.alias === alias);
	}
}

export const umbPropertyEditorData = new UmbPropertyEditorData();
