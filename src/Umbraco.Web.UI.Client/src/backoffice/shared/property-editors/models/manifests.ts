import { manifest as colorPicker } from './Umbraco.ColorPicker';
import { manifest as eyeDropper } from './Umbraco.ColorPicker.EyeDropper';
import { manifest as contentPicker } from './Umbraco.ContentPicker';
import { manifest as json } from './Umbraco.JSON';
import { manifest as multiUrlPicker } from './Umbraco.MultiUrlPicker';

import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifests: Array<ManifestPropertyEditorModel> = [
	colorPicker,
	eyeDropper,
	contentPicker,
	json,
	multiUrlPicker,
	{
		type: 'propertyEditorModel',
		name: 'Multi URL Picker',
		alias: 'Umbraco.MultiUrlPicker',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Multinode Treepicker',
		alias: 'Umbraco.MultiNodeTreePicker',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Date/Time',
		alias: 'Umbraco.DateTime',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Decimal',
		alias: 'Umbraco.Decimal',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Email address',
		alias: 'Umbraco.EmailAddress',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Label',
		alias: 'Umbraco.Label',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Numeric',
		alias: 'Umbraco.Integer',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Slider',
		alias: 'Umbraco.Slider',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Tags',
		alias: 'Umbraco.Tags',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Textarea',
		alias: 'Umbraco.TextArea',
		meta: {
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
	},
	{
		type: 'propertyEditorModel',
		name: 'Textbox',
		alias: 'Umbraco.TextBox',
		meta: {
			config: {
				properties: [
					{
						alias: 'maxChars',
						label: 'Maximum allowed characters',
						description: 'If empty, 512 character limit',
						propertyEditorUI: 'Umb.PropertyEditorUI.Number',
					},
				],
				defaultData: [
					{
						alias: 'maxChars',
						value: 512,
					},
				],
			},
		},
	},
	{
		type: 'propertyEditorModel',
		name: 'Toggle',
		alias: 'Umbraco.TrueFalse',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Markdown editor',
		alias: 'Umbraco.MarkdownEditor',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Rich Text Editor',
		alias: 'Umbraco.TinyMCE',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Member Group Picker',
		alias: 'Umbraco.MemberGroupPicker',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Member Picker',
		alias: 'Umbraco.MemberPicker',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'User Picker',
		alias: 'Umbraco.UserPicker',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Block Grid',
		alias: 'Umbraco.BlockGrid',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Block List',
		alias: 'Umbraco.BlockList',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Checkbox list',
		alias: 'Umbraco.CheckBoxList',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Dropdown',
		alias: 'Umbraco.DropDown.Flexible',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'List view',
		alias: 'Umbraco.ListView',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Radio button list',
		alias: 'Umbraco.RadioButtonList',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Repeatable textstrings',
		alias: 'Umbraco.MultipleTextstring',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'File upload',
		alias: 'Umbraco.UploadField',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Image Cropper',
		alias: 'Umbraco.ImageCropper',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Media Picker',
		alias: 'Umbraco.MediaPicker3',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Icon Picker',
		alias: 'Umbraco.IconPicker',
		meta: {},
	},
];
