import { manifest as colorPicker } from './color-picker/manifests';
import { manifest as contentPicker } from './content-picker/manifests';
import { manifest as datePicker } from './date-picker/manifests';
import { manifest as eyeDropper } from './eye-dropper/manifests';
import { manifest as multiUrlPicker } from './multi-url-picker/manifests';
import { manifest as overlaySize } from './overlay-size/manifests';
import { manifest as treePicker } from './tree-picker/manifests';
import { manifest as treePickerStartNode } from './tree-picker-start-node/manifests';
import { manifests as textBoxes } from './text-box/manifests';
import { manifest as dropdown } from './dropdown/manifests';
import { manifest as multipleTextString } from './multiple-text-string/manifests';
import { manifest as textArea } from './textarea/manifests';
import { manifest as slider } from './slider/manifests';
import { manifest as toggle } from './toggle/manifests';
import { manifest as tags } from './tags/manifests';
import { manifest as markdownEditor } from './markdown-editor/manifests';
import { manifest as radioButtonList } from './radio-button-list/manifests';
import { manifest as checkboxList } from './checkbox-list/manifests';
import { manifest as blockList } from './block-list/manifests';
import { manifest as numberRange } from './number-range/manifests';
import { manifest as blockListBlockConfiguration } from './block-list-block-configuration/manifests';
import { manifest as mediaPicker } from './media-picker/manifests';
import { manifest as imageCropsConfiguration } from './image-crops-configuration/manifests';
import { manifest as imageCropper } from './image-cropper/manifests';
import { manifest as uploadField } from './upload-field/manifests';
import { manifests as blockGrid } from './block-grid/manifests';
import { manifest as orderDirection } from './order-direction/manifests';
import { manifests as collectionView } from './collection-view/manifests';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifests: Array<ManifestPropertyEditorUI> = [
	colorPicker,
	contentPicker,
	datePicker,
	eyeDropper,
	multiUrlPicker,
	overlaySize,
	...textBoxes,
	treePicker,
	treePickerStartNode,
	dropdown,
	multipleTextString,
	textArea,
	slider,
	toggle,
	tags,
	markdownEditor,
	radioButtonList,
	checkboxList,
	blockList,
	numberRange,
	blockListBlockConfiguration,
	mediaPicker,
	imageCropsConfiguration,
	imageCropper,
	uploadField,
	...blockGrid,
	orderDirection,
	...collectionView,
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
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.IconPicker',
		name: 'Icon Picker Property Editor UI',
		loader: () => import('./icon-picker/property-editor-ui-icon-picker.element'),
		meta: {
			label: 'Icon Picker',
			propertyEditorModel: 'Umbraco.IconPicker',
			icon: 'umb:document',
			group: 'common',
		},
	},
];
