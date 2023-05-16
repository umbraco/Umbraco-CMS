import { manifest as colorPicker } from './color-picker/manifests';
import { manifest as datePicker } from './date-picker/manifests';
import { manifest as eyeDropper } from './eye-dropper/manifests';
import { manifest as multiUrlPicker } from './multi-url-picker/manifests';
import { manifest as overlaySize } from './overlay-size/manifests';
import { manifests as treePicker } from './tree-picker/manifests';
import { manifests as textBoxes } from './text-box/manifests';
import { manifest as dropdown } from './dropdown/manifests';
import { manifest as multipleTextString } from './multiple-text-string/manifests';
import { manifest as textArea } from './textarea/manifests';
import { manifest as slider } from './slider/manifests';
import { manifest as toggle } from './toggle/manifests';
import { manifest as markdownEditor } from './markdown-editor/manifests';
import { manifest as radioButtonList } from './radio-button-list/manifests';
import { manifest as checkboxList } from './checkbox-list/manifests';
import { manifests as blockList } from './block-list/manifests';
import { manifest as numberRange } from './number-range/manifests';
import { manifest as mediaPicker } from './media-picker/manifests';
import { manifest as imageCropsConfiguration } from './image-crops-configuration/manifests';
import { manifest as imageCropper } from './image-cropper/manifests';
import { manifest as uploadField } from './upload-field/manifests';
import { manifests as blockGrid } from './block-grid/manifests';
import { manifest as orderDirection } from './order-direction/manifests';
import { manifests as collectionView } from './collection-view/manifests';
import { manifests as tinyMCE } from './tiny-mce/manifests';
import { manifest as iconPicker } from './icon-picker/manifests';
import { manifest as label } from './label/manifests';
import { manifest as valueType } from './value-type/manifests';
import { manifests as numbers } from './number/manifests';
import { manifest as userPicker } from './user-picker/manifests';
import { manifest as memberPicker } from './member-picker/manifests';
import { manifest as memberGroupPicker } from './member-group-picker/manifests';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestPropertyEditorUI> = [
	colorPicker,
	datePicker,
	eyeDropper,
	multiUrlPicker,
	overlaySize,
	dropdown,
	multipleTextString,
	textArea,
	slider,
	toggle,
	markdownEditor,
	radioButtonList,
	checkboxList,
	numberRange,
	mediaPicker,
	imageCropsConfiguration,
	imageCropper,
	uploadField,
	orderDirection,
	iconPicker,
	label,
	valueType,
	userPicker,
	memberPicker,
	memberGroupPicker,
	...numbers,
	...textBoxes,
	...treePicker,
	...blockList,
	...blockGrid,
	...collectionView,
	...tinyMCE,
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Number',
		name: 'Number Property Editor UI',
		loader: () => import('./number/property-editor-ui-number.element'),
		meta: {
			label: 'Number',
			icon: 'umb:autofill',
			group: 'common',
			propertyEditorModel: 'Umbraco.Integer',
		},
	},
];
