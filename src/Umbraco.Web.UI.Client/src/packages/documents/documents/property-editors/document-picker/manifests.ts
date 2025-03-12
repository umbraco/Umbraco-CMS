import { manifest as schemaManifest } from './Umbraco.ContentPicker.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DocumentPicker',
		name: 'Document Picker Property Editor UI',
		element: () => import('./property-editor-ui-document-picker.element.js'),
		meta: {
			label: 'Document Picker',
			propertyEditorSchemaAlias: 'Umbraco.ContentPicker',
			icon: 'icon-document',
			group: 'pickers',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'startNodeId',
						label: 'Limit access',
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
	},
	schemaManifest,
];
