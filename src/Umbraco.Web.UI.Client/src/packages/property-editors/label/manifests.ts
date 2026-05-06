import { manifest as labelSchemaManifest } from './Umbraco.Label.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Label',
		name: 'Label Property Editor UI',
		element: () => import('./property-editor-ui-label.element.js'),
		meta: {
			label: 'Label',
			icon: 'icon-readonly',
			group: 'common',
			propertyEditorSchemaAlias: 'Umbraco.Label',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'labelTemplate',
						label: 'Label template',
						description: 'Enter a template for the label.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
			},
		},
	},
	labelSchemaManifest,
];
