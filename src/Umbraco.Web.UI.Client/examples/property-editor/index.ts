import type { ManifestPropertyEditorUi } from '@umbraco-cms/backoffice/property-editor';

export const manifests: Array<ManifestPropertyEditorUi> = [
	{
		type: 'propertyEditorUi',
		alias: 'example.propertyEditorUi.propertyEditor',
		name: 'Example Property Editor UI',
		element: () => import('./property-editor.js'),
		meta: {
			label: 'Example Editor',
			propertyEditorSchemaAlias: 'Umbraco.ListView',
			icon: 'icon-code',
			group: 'common',
			settings: {
				properties: [
					{
						alias: 'customText',
						label: 'Custom text',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
				defaultData: [
					{
						alias: 'customText',
						value: 'Default value',
					},
				],
			},
		},
	},
];
