import { manifest as schemaManifest } from './Umbraco.ElementPicker.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const propertyEditorUi: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ElementPicker',
	name: 'Element Picker Property Editor UI',
	element: () => import('./element-picker-property-editor-ui.element.js'),
	meta: {
		label: schemaManifest.name,
		propertyEditorSchemaAlias: schemaManifest.alias,
		icon: 'icon-plugin',
		group: 'pickers',
		supportsReadOnly: true,
		settings: {
			properties: [
				{
					alias: 'allowedContentTypes',
					label: 'Accepted types',
					description: 'Limit to specific types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentTypePicker',
					config: [{ alias: 'onlyPickElementTypes', value: true }],
					weight: 10,
				},
				{
					alias: 'validationLimit',
					label: 'Amount',
					description: 'Set a required range of items',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
					weight: 20,
				},
				{
					alias: 'startNodeId',
					label: 'Start node',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ElementPicker',
					config: [
						{ alias: 'folderOnly', value: true },
						{ alias: 'validationLimit', value: { min: 0, max: 1 } },
					],
					weight: 30,
				},
			],
		},
	},
};

export const manifests: Array<UmbExtensionManifest> = [propertyEditorUi, schemaManifest];
