import { UmbData } from './data';
import type { PropertyEditor } from '../../core/models';
import { cloneDeep } from 'lodash';

export const data: Array<PropertyEditor> = [
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Color Picker',
		icon: 'umb:colorpicker',
		alias: 'Umbraco.ColorPicker',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Content Picker',
		icon: 'umb:autofill',
		alias: 'Umbraco.ContentPicker',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Eye Dropper Color Picker',
		icon: 'umb:colorpicker',
		alias: 'Umbraco.ColorPicker.EyeDropper',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Form Picker',
		icon: 'umb:umb-contour',
		alias: 'UmbracoForms.FormPicker',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: false,
		name: 'Form Theme Picker',
		icon: 'umb:brush',
		alias: 'UmbracoForms.ThemePicker',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Multi URL Picker',
		icon: 'umb:link',
		alias: 'Umbraco.MultiUrlPicker',
	},
	{
		isSystem: false,
		group: 'Pickers',
		hasConfig: true,
		name: 'Multinode Treepicker',
		icon: 'umb:page-add',
		alias: 'Umbraco.MultiNodeTreePicker',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Date/Time',
		icon: 'umb:time',
		alias: 'Umbraco.DateTime',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Decimal',
		icon: 'umb:autofill',
		alias: 'Umbraco.Decimal',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Email address',
		icon: 'umb:message',
		alias: 'Umbraco.EmailAddress',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Label',
		icon: 'umb:readonly',
		alias: 'Umbraco.Label',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Numeric',
		icon: 'umb:autofill',
		alias: 'Umbraco.Integer',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Open Street Map',
		icon: 'umb:map-location',
		alias: 'Bergmania.OpenStreetMap',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Slider',
		icon: 'umb:navigation-horizontal',
		alias: 'Umbraco.Slider',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Tags',
		icon: 'umb:tags',
		alias: 'Umbraco.Tags',
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Textarea',
		icon: 'umb:application-window-alt',
		alias: 'Umbraco.TextArea',
		config: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty - no character limit',
					propertyEditorUI: 'Umb.PropertyEditorUI.Number',
				},
			],
		},
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Textbox',
		icon: 'umb:autofill',
		alias: 'Umbraco.TextBox',
		config: {
			properties: [
				{
					alias: 'maxChars',
					label: 'Maximum allowed characters',
					description: 'If empty, 512 character limit',
					propertyEditorUI: 'Umb.PropertyEditorUI.Textarea',
				},
			],
			defaultData: {
				maxChars: 512,
			},
		},
	},
	{
		isSystem: false,
		group: 'Common',
		hasConfig: true,
		name: 'Toggle',
		icon: 'umb:checkbox',
		alias: 'Umbraco.TrueFalse',
	},
	{
		isSystem: false,
		group: 'Rich Content',
		hasConfig: true,
		name: 'Grid layout',
		icon: 'umb:layout',
		alias: 'Umbraco.Grid',
	},
	{
		isSystem: false,
		group: 'Rich Content',
		hasConfig: true,
		name: 'Markdown editor',
		icon: 'umb:code',
		alias: 'Umbraco.MarkdownEditor',
	},
	{
		isSystem: false,
		group: 'Rich Content',
		hasConfig: true,
		name: 'Rich Text Editor',
		icon: 'umb:browser-window',
		alias: 'Umbraco.TinyMCE',
	},
	{
		isSystem: false,
		group: 'People',
		hasConfig: false,
		name: 'Member Group Picker',
		icon: 'umb:users-alt',
		alias: 'Umbraco.MemberGroupPicker',
	},
	{
		isSystem: false,
		group: 'People',
		hasConfig: false,
		name: 'Member Picker',
		icon: 'umb:user',
		alias: 'Umbraco.MemberPicker',
	},
	{
		isSystem: false,
		group: 'People',
		hasConfig: false,
		name: 'User Picker',
		icon: 'umb:user',
		alias: 'Umbraco.UserPicker',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Block Grid',
		icon: 'umb:thumbnail-list',
		alias: 'Umbraco.BlockGrid',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Block List',
		icon: 'umb:thumbnail-list',
		alias: 'Umbraco.BlockList',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Checkbox list',
		icon: 'umb:bulleted-list',
		alias: 'Umbraco.CheckBoxList',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Dropdown',
		icon: 'umb:indent',
		alias: 'Umbraco.DropDown.Flexible',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'List view',
		icon: 'umb:thumbnail-list',
		alias: 'Umbraco.ListView',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Nested Content',
		icon: 'umb:thumbnail-list',
		alias: 'Umbraco.NestedContent',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Radio button list',
		icon: 'umb:target',
		alias: 'Umbraco.RadioButtonList',
	},
	{
		isSystem: false,
		group: 'Lists',
		hasConfig: true,
		name: 'Repeatable textstrings',
		icon: 'umb:ordered-list',
		alias: 'Umbraco.MultipleTextstring',
	},
	{
		isSystem: false,
		group: 'Media',
		hasConfig: true,
		name: 'File upload',
		icon: 'umb:download-alt',
		alias: 'Umbraco.UploadField',
	},
	{
		isSystem: false,
		group: 'Media',
		hasConfig: true,
		name: 'Image Cropper',
		icon: 'umb:crop',
		alias: 'Umbraco.ImageCropper',
	},
	{
		isSystem: false,
		group: 'Media',
		hasConfig: true,
		name: 'Media Picker',
		icon: 'umb:picture',
		alias: 'Umbraco.MediaPicker3',
	},
	{
		isSystem: false,
		group: 'Media',
		hasConfig: true,
		name: 'Media Picker (legacy)',
		icon: 'umb:picture',
		alias: 'Umbraco.MediaPicker',
	},
	{
		isSystem: false,
		group: 'Custom',
		hasConfig: true,
		name: 'Custom Property Editor',
		icon: 'umb:autofill',
		alias: 'Umbraco.Custom',
	},
];

// Temp mocked database
class UmbPropertyEditorData extends UmbData<PropertyEditor> {
	constructor() {
		super(data);
	}

	getAll() {
		const clonedEditors = cloneDeep(this.data);
		clonedEditors.forEach((editor) => delete editor.config);
		return clonedEditors;
	}

	getByAlias(alias: string) {
		const editor = this.data.find((e) => e.alias === alias);
		const clone = cloneDeep(editor);
		delete clone?.config;
		return clone;
	}

	getConfig(alias: string) {
		const editor = this.data.find((e) => e.alias === alias);
		return editor?.config ?? undefined;
	}
}

export const umbPropertyEditorData = new UmbPropertyEditorData();
