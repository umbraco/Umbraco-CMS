import { manifests as schemaManifests } from './Umbraco.MultipleTextString.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
