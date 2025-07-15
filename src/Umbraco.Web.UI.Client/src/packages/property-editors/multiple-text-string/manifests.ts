import { manifests as schemaManifests } from './Umbraco.MultipleTextString.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MultipleTextString',
		name: 'Multiple Text String Property Editor UI',
		element: () => import('./property-editor-ui-multiple-text-string.element.js'),
		meta: {
			label: 'Multiple Text String',
			propertyEditorSchemaAlias: 'Umbraco.MultipleTextstring',
			icon: 'icon-ordered-list',
			group: 'lists',
			supportsReadOnly: true,
		},
	},
	...schemaManifests,
];
