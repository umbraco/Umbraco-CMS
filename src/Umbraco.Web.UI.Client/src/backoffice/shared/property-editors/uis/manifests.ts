import { manifest as ColorPicker } from './color-picker/manifests';
import { manifest as ContentPicker } from './content-picker/manifests';
import { manifest as EyeDropper } from './eye-dropper/manifests';
import { manifest as TreePicker } from './tree-picker/manifests';
import { manifest as TreePickerStartNode } from './tree-picker-start-node/manifests';
import { manifest as MultiUrlPicker } from './multi-url-picker/manifests';
import { manifest as OverlaySize } from './overlay-size/manifests';
import type { ManifestPropertyEditorUI } from '@umbraco-cms/models';

export const manifests: Array<ManifestPropertyEditorUI> = [
	ColorPicker,
	ContentPicker,
	EyeDropper,
	TreePicker,
	TreePickerStartNode,
	MultiUrlPicker,
	OverlaySize,
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
		alias: 'Umb.PropertyEditorUI.TextBox',
		name: 'Text Property Editor UI',
		loader: () => import('./text-box/property-editor-ui-text-box.element'),
		meta: {
			label: 'Text',
			icon: 'umb:edit',
			group: 'common',
			propertyEditorModel: 'Umbraco.TextBox',
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
