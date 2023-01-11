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
import { manifest as trueFalse } from './Umbraco.TrueFalse';
import { manifest as tags } from './Umbraco.Tags';
import { manifest as markdownEditor } from './Umbraco.MarkdownEditor';
import { manifest as radioButtonList } from './Umbraco.RadioButtonList';
import { manifest as blockList } from './Umbraco.BlockList';
import { manifest as checkboxList } from './Umbraco.CheckboxList';
import { manifest as mediaPicker } from './Umbraco.MediaPicker3';
import { manifest as imageCropper } from './Umbraco.ImageCropper';
import { manifest as uploadField } from './Umbraco.UploadField';
import { manifest as blockGrid } from './Umbraco.BlockGrid';

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
	trueFalse,
	tags,
	markdownEditor,
	radioButtonList,
	checkboxList,
	blockList,
	mediaPicker,
	imageCropper,
	uploadField,
	blockGrid,
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
		name: 'List view',
		alias: 'Umbraco.ListView',
		meta: {},
	},
	{
		type: 'propertyEditorModel',
		name: 'Icon Picker',
		alias: 'Umbraco.IconPicker',
		meta: {},
	},
];
