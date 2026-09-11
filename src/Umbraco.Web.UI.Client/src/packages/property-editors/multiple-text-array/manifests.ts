import { manifests as schemaManifests } from './Umbraco.MultipleTextArray.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MultipleTextArray',
		name: 'Multiple Text Array Property Editor UI',
		element: () => import('./property-editor-ui-multiple-text-array.element.js'),
		meta: {
			label: 'Multiple Text Array',
			propertyEditorSchemaAlias: 'Umbraco.MultipleTextArray',
			icon: 'icon-ordered-list',
			group: '#propertyEditorUIGroups_lists',
			keywords: ['string', 'list', 'items', 'values', 'features', 'tags', 'keywords', 'bullets', 'entries', 'array'],
			supportsReadOnly: true,
		},
	},
	...schemaManifests,
];
