import { manifest as schemaManifest } from './Umbraco.Tags.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
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
		},
	},
	schemaManifest,
];
