import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentPicker',
	name: 'Document Picker Property Editor UI',
	js: () => import('./property-editor-ui-document-picker.element.js'),
	meta: {
		label: 'Document Picker',
		propertyEditorSchemaAlias: 'Umbraco.ContentPicker',
		icon: 'icon-document',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'startNodeId',
					label: 'Start node',
					description: '',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.DocumentPicker',
					config: [
						{
							alias: 'validationLimit',
							value: { min: 0, max: 1 },
						},
					],
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
