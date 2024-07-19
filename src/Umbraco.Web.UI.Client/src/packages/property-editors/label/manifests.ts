import { manifest as labelSchemaManifest } from './Umbraco.Label.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Label',
		name: 'Label Property Editor UI',
		element: () => import('./property-editor-ui-label.element.js'),
		meta: {
			label: 'Label',
			icon: 'icon-readonly',
			group: 'pickers',
			propertyEditorSchemaAlias: 'Umbraco.Label',
		},
	},
	labelSchemaManifest,
];
