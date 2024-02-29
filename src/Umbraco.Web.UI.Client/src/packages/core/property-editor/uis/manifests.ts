import { manifest as checkboxList } from './checkbox-list/manifests.js';
import { manifest as codeEditor } from './code-editor/manifests.js';
import { manifest as colorEditor } from './color-swatches-editor/manifests.js';
import { manifest as colorPicker } from './color-picker/manifests.js';
import { manifest as datePicker } from './date-picker/manifests.js';
import { manifest as dropdown } from './dropdown/manifests.js';
import { manifest as eyeDropper } from './eye-dropper/manifests.js';
import { manifest as iconPicker } from './icon-picker/manifests.js';
import { manifest as label } from './label/manifests.js';
import { manifest as markdownEditor } from './markdown-editor/manifests.js';
import { manifest as memberGroupPicker } from './member-group-picker/manifests.js';
import { manifest as memberPicker } from './member-picker/manifests.js';
import { manifest as multipleTextString } from './multiple-text-string/manifests.js';
import { manifest as multiUrlPicker } from './multi-url-picker/manifests.js';
import { manifest as numberRange } from './number-range/manifests.js';
import { manifest as orderDirection } from './order-direction/manifests.js';
import { manifest as overlaySize } from './overlay-size/manifests.js';
import { manifest as radioButtonList } from './radio-button-list/manifests.js';
import { manifest as slider } from './slider/manifests.js';
import { manifest as textArea } from './textarea/manifests.js';
import { manifest as toggle } from './toggle/manifests.js';
import { manifest as uploadField } from './upload-field/manifests.js';
import { manifest as userPicker } from './user-picker/manifests.js';
import { manifest as valueType } from './value-type/manifests.js';
import { manifests as collectionView } from './collection-view/manifests.js';
import { manifests as numbers } from './number/manifests.js';
import { manifests as textBoxes } from './text-box/manifests.js';
import { manifests as treePicker } from './tree-picker/manifests.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyEditorUi> = [
	checkboxList,
	codeEditor,
	colorEditor,
	colorPicker,
	datePicker,
	dropdown,
	eyeDropper,
	iconPicker,
	label,
	markdownEditor,
	memberGroupPicker,
	memberPicker,
	multipleTextString,
	multiUrlPicker,
	numberRange,
	orderDirection,
	overlaySize,
	radioButtonList,
	slider,
	textArea,
	toggle,
	uploadField,
	userPicker,
	valueType,
	...collectionView,
	...numbers,
	...textBoxes,
	...treePicker,
];
