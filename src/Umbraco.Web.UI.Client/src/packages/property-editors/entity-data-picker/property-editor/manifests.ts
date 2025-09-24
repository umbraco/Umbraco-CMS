import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.EntityDataPicker',
	name: 'Entity Data Picker Property Editor UI',
	element: () => import('./entity-data-picker-property-editor-ui.element.js'),
	meta: {
		label: 'Entity Data Picker',
		icon: 'icon-page-add',
		group: 'pickers',
		propertyEditorSchemaAlias: 'Umbraco.Plain.Json',
		supportsReadOnly: true,
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
