import { manifest as schemaManifest } from './Umbraco.TextArea.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.TextArea',
		name: 'Text Area Property Editor UI',
		element: () => import('./property-editor-ui-textarea.element.js'),
		meta: {
			label: 'Text Area',
			propertyEditorSchemaAlias: 'Umbraco.TextArea',
			icon: 'icon-edit',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'rows',
						label: 'Number of rows',
						description: 'If empty or zero, the textarea is set to auto-height',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
					},
					{
						alias: 'minHeight',
						label: 'Min height (pixels)',
						description: 'Sets the minimum height of the textarea',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
					},
					{
						alias: 'maxHeight',
						label: 'Max height (pixels)',
						description: 'Sets the maximum height of the textarea',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Integer',
						config: [{ alias: 'min', value: 0 }],
					},
				],
				defaultData: [{ alias: 'rows', value: 10 }],
			},
		},
	},
	schemaManifest,
];
