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
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.BlockList',
		name: 'Block List Property Editor UI',
		loader: () => import('./block-list/property-editor-ui-block-list.element'),
		meta: {
			label: 'Block List',
			icon: 'umb:thumbnail-list',
			group: 'lists',
			propertyEditorModel: 'Umbraco.BlockList',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Toggle',
		name: 'Toggle Property Editor UI',
		loader: () => import('./toggle/property-editor-ui-toggle.element'),
		meta: {
			label: 'Toggle',
			icon: 'umb:checkbox',
			group: 'common',
			propertyEditorModel: 'Umbraco.TrueFalse',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.CheckboxList',
		name: 'Checkbox List Property Editor UI',
		loader: () => import('./checkbox-list/property-editor-ui-checkbox-list.element'),
		meta: {
			label: 'Checkbox List',
			icon: 'umb:bulleted-list',
			group: 'lists',
			propertyEditorModel: 'Umbraco.CheckBoxList',
		},
	},
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUI.Textarea',
		name: 'Textarea Property Editor UI',
		loader: () => import('./textarea/property-editor-ui-textarea.element'),
		meta: {
			label: 'Textarea',
			icon: 'umb:edit',
			group: 'common',
			propertyEditorModel: 'Umbraco.TextArea',
			config: {
				properties: [
					{
						alias: 'rows',
						label: 'Number of rows',
						description: 'If empty - 10 rows would be set as the default value',
						propertyEditorUI: 'Umb.PropertyEditorUI.Number',
					},
				],
			},
		},
	},
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
