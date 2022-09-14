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
		icon: 'icon-thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.BlockList',
		name: 'Block List',
		icon: 'icon-thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.CheckBoxList',
		name: 'Checkbox list',
		icon: 'icon-bulleted-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ColorPicker',
		name: 'Color Picker',
		icon: 'icon-colorpicker',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ContentPicker',
		name: 'Content Picker',
		icon: 'icon-autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.DateTime',
		name: 'Date/Time',
		icon: 'icon-time',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Decimal',
		name: 'Decimal',
		icon: 'icon-autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.DropDown.Flexible',
		name: 'Dropdown',
		icon: 'icon-indent',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.EmailAddress',
		name: 'Email address',
		icon: 'icon-message',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ColorPicker.EyeDropper',
		name: 'Eye Dropper Color Picker',
		icon: 'icon-colorpicker',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.UploadField',
		name: 'File upload',
		icon: 'icon-download-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Grid',
		name: 'Grid layout',
		icon: 'icon-layout',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ImageCropper',
		name: 'Image Cropper',
		icon: 'icon-crop',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Label',
		name: 'Label',
		icon: 'icon-readonly',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.ListView',
		name: 'List view',
		icon: 'icon-thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MarkdownEditor',
		name: 'Markdown editor',
		icon: 'icon-code',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MediaPicker3',
		name: 'Media Picker',
		icon: 'icon-picture',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MediaPicker',
		name: 'Media Picker (legacy)',
		icon: 'icon-picture',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MemberGroupPicker',
		name: 'Member Group Picker',
		icon: 'icon-users-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MemberPicker',
		name: 'Member Picker',
		icon: 'icon-user',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultiUrlPicker',
		name: 'Multi URL Picker',
		icon: 'icon-link',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultiNodeTreePicker',
		name: 'Multinode Treepicker',
		icon: 'icon-page-add',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.NestedContent',
		name: 'Nested Content',
		icon: 'icon-thumbnail-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Integer',
		name: 'Numeric',
		icon: 'icon-autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.RadioButtonList',
		name: 'Radio button list',
		icon: 'icon-target',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.MultipleTextstring',
		name: 'Repeatable textstrings',
		icon: 'icon-ordered-list',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TinyMCE',
		name: 'Rich Text Editor',
		icon: 'icon-browser-window',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Slider',
		name: 'Slider',
		icon: 'icon-navigation-horizontal',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.Tags',
		name: 'Tags',
		icon: 'icon-tags',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TextArea',
		name: 'Textarea',
		icon: 'icon-application-window-alt',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TextBox',
		name: 'Textbox',
		icon: 'icon-autofill',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.TrueFalse',
		name: 'Toggle',
		icon: 'icon-checkbox',
		editor: {
			view: '',
		},
	},
	{
		alias: 'Umbraco.UserPicker',
		name: 'User Picker',
		icon: 'icon-user',
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
}

export const umbPropertyEditorData = new UmbPropertyEditorData();
