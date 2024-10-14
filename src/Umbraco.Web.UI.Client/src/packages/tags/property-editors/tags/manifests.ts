import { manifest as schemaManifest } from './Umbraco.Tags.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Tags',
		name: 'Tags Property Editor UI',
		element: () => import('./property-editor-ui-tags.element.js'),
		meta: {
			label: 'Tags',
			propertyEditorSchemaAlias: 'Umbraco.Tags',
			icon: 'icon-tags',
			group: 'common',
			supportsReadOnly: true,
		},
	},
	schemaManifest,
];
