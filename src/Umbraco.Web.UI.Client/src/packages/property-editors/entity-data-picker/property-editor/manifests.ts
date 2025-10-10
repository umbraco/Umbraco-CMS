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
		supportsDataSource: {
			enabled: true,
			forDataSourceTypes: ['picker'],
		},
		settings: {
			properties: [
				// TODO: Move this to schema manifest when server can validate it
				{
					alias: 'validationLimit',
					label: 'Amount',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
					config: [{ alias: 'validationRange', value: { min: 0, max: Infinity } }],
					weight: 100,
				},
			],
		},
	},
};

export const manifests: Array<UmbExtensionManifest> = [manifest];
