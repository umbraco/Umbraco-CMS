import { manifest as schemaManifest } from './Umbraco.ElementPicker.js';
import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const propertyEditorUi: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.ElementPicker',
	name: 'Element Picker Property Editor UI',
	element: () => import('./property-editor-ui-element-picker.element.js'),
	meta: {
		label: schemaManifest.name,
		propertyEditorSchemaAlias: schemaManifest.alias,
		icon: 'icon-page-add',
		group: 'pickers',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [propertyEditorUi, schemaManifest];
