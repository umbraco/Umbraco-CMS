import { manifest as schemaManifest } from './Umbraco.Dropdown.Flexible.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Dropdown',
		name: 'Dropdown Property Editor UI',
		element: () => import('./property-editor-ui-dropdown.element.js'),
		meta: {
			label: 'Dropdown',
			propertyEditorSchemaAlias: 'Umbraco.DropDown.Flexible',
			icon: 'icon-list',
			group: 'lists',
		},
	},
	schemaManifest,
];
