import { manifest as blockGrid } from './Umbraco.BlockGrid.js';
import { manifest as blockList } from './Umbraco.BlockList.js';
import { manifest as checkboxList } from './Umbraco.CheckboxList.js';
import { manifest as colorPicker } from './Umbraco.ColorPicker.js';
import { manifest as dateTime } from './Umbraco.DateTime.js';
import { manifest as decimal } from './Umbraco.Decimal.js';
import { manifest as dropdownFlexible } from './Umbraco.Dropdown.Flexible.js';
import { manifest as emailAddress } from './Umbraco.EmailAddress.js';
import { manifest as eyeDropper } from './Umbraco.ColorPicker.EyeDropper.js';
import { manifest as iconPicker } from './Umbraco.IconPicker.js';
import { manifest as imageCropper } from './Umbraco.ImageCropper.js';
import { manifest as integer } from './Umbraco.Integer.js';
import { manifest as json } from './Umbraco.JSON.js';
import { manifest as label } from './Umbraco.Label.js';
import { manifest as listView } from './Umbraco.ListView.js';
import { manifest as markdownEditor } from './Umbraco.MarkdownEditor.js';
import { manifest as mediaPicker } from './Umbraco.MediaPicker3.js';
import { manifest as memberGroupPicker } from './Umbraco.MemberGroupPicker.js';
import { manifest as memberPicker } from './Umbraco.MemberPicker.js';
import { manifest as multiNodeTreePicker } from './Umbraco.MultiNodeTreePicker.js';
import { manifest as multipleTextString } from './Umbraco.MultipleTextString.js';
import { manifest as multiUrlPicker } from './Umbraco.MultiUrlPicker.js';
import { manifest as radioButtonList } from './Umbraco.RadioButtonList.js';
import { manifest as slider } from './Umbraco.Slider.js';
import { manifest as tags } from './Umbraco.Tags.js';
import { manifest as textArea } from './Umbraco.TextArea.js';
import { manifest as textBox } from './Umbraco.TextBox.js';
import { manifest as tinyMCE } from './Umbraco.TinyMCE.js';
import { manifest as trueFalse } from './Umbraco.TrueFalse.js';
import { manifest as uploadField } from './Umbraco.UploadField.js';
import { manifest as userPicker } from './Umbraco.UserPicker.js';

import type { ManifestPropertyEditorSchema } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyEditorSchema> = [
	blockGrid,
	blockList,
	checkboxList,
	colorPicker,
	dateTime,
	decimal,
	dropdownFlexible,
	emailAddress,
	eyeDropper,
	iconPicker,
	imageCropper,
	integer,
	json,
	label,
	listView,
	markdownEditor,
	mediaPicker,
	memberGroupPicker,
	memberPicker,
	multiNodeTreePicker,
	multipleTextString,
	multiUrlPicker,
	radioButtonList,
	slider,
	tags,
	textArea,
	textBox,
	tinyMCE,
	trueFalse,
	uploadField,
	userPicker,
];
