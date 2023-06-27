import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/extension-registry';

export const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.DocumentPicker',
	name: 'Document Picker Property Editor UI',
	loader: () => import('./property-editor-ui-document-picker.element.js'),
	meta: {
		label: 'Document Picker',
		propertyEditorSchemaAlias: 'Umbraco.ContentPicker',
		icon: 'umb:document',
		group: 'common',
		settings: {
			properties: [
				{
					alias: 'showOpenButton',
					label: 'Show open button',
					description: 'Opens the node in a dialog',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
				{
					alias: 'validationLimit',
					label: 'Amount of Documents',
					description: 'Require a certain amount of documents',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.NumberRange',
				},
				{
					alias: 'ignoreUserStartNodes',
					label: 'Ignore User Start Nodes',
					description: 'Selecting this option allows a user to choose nodes that they normally dont have access to',
					propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
				},
			],
		},
	},
};
