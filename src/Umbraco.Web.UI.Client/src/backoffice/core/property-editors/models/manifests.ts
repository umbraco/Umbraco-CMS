import { manifest as colorPicker } from './Umbraco.ColorPicker';
import { manifest as eyeDropper } from './Umbraco.ColorPicker.EyeDropper';
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
import { manifest as tinyMCE } from './Umbraco.TinyMCE';
import { manifest as listView } from './Umbraco.ListView';
import { manifest as label } from './Umbraco.Label';
import { manifest as integer } from './Umbraco.Integer';
import { manifest as decimal } from './Umbraco.Decimal';
import { manifest as userPicker } from './Umbraco.UserPicker';
import { manifest as memberPicker } from './Umbraco.MemberPicker';
import { manifest as memberGroupPicker } from './Umbraco.MemberGroupPicker';

import type { ManifestPropertyEditorModel } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyEditorModel> = [
	colorPicker,
	eyeDropper,
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
	tinyMCE,
	listView,
	label,
	integer,
	decimal,
	userPicker,
	memberPicker,
	memberGroupPicker,
	{
		type: 'propertyEditorModel',
		name: 'Icon Picker',
		alias: 'Umbraco.IconPicker',
		meta: {},
	},
];
