import { manifest as colorPicker } from './Umbraco.ColorPicker';
import { manifest as eyeDropper } from './Umbraco.ColorPicker.EyeDropper';
import { manifest as contentPicker } from './Umbraco.ContentPicker';
import { manifest as json } from './Umbraco.JSON';
import { manifest as multiUrlPicker } from './Umbraco.MultiUrlPicker';
import { manifest as multiNodeTreePicker } from './Umbraco.MultiNodeTreePicker';
import { manifest as dateTime } from './Umbraco.DateTime';
import { manifest as emailAddress } from './Umbraco.EmailAddress';
import { manifest as dropdownFlexible } from './Umbraco.Dropdown.Flexible';
import { manifest as textBox } from './Umbraco.TextBox';
import { manifest as multipleTextString } from './Umbraco.MultipleTextString';
import { manifest as textArea } from './Umbraco.TextArea';
import { manifest as slider } from './Umbraco.Slider';

import type { ManifestPropertyEditorModel } from '@umbraco-cms/models';

export const manifests: Array<ManifestPropertyEditorModel> = [
	colorPicker,
	eyeDropper,
	contentPicker,
	dateTime,
	emailAddress,
	json,
	multiUrlPicker,
	multiNodeTreePicker,
	dropdownFlexible,
	textBox,
	multipleTextString,
	textArea,
	slider,
	{
		type: 'propertyEditorModel',
		name: 'Decimal',
		alias: 'Umbraco.Decimal',
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
		name: 'Tags',
		alias: 'Umbraco.Tags',
		meta: {},
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
