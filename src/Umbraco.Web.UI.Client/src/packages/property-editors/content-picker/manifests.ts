import { manifest as sourcePicker } from './config/source/manifests.js';
import { manifest as sourceTypePicker } from './config/source-type-picker/manifests.js';
import { manifest as schemaManifest } from './Umbraco.MultiNodeTreePicker.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ContentPicker',
	name: 'Content Picker Property Editor UI',
	element: () => import('./property-editor-ui-content-picker.element.js'),
	meta: {
		label: 'Content Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: 'Umbraco.MultiNodeTreePicker',
		settings: {
			properties: [
				{
					alias: 'filter',
					label: 'Allow items of type',
					description: 'Select the applicable types',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.ContentPicker.SourceType',
				},
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};

const config: Array<ManifestPropertyEditorUi> = [sourcePicker, sourceTypePicker];

export const manifests: Array<ManifestTypes> = [manifest, ...config, schemaManifest];
