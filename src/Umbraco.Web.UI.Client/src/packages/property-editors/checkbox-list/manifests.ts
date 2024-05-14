import { manifest as schemaManifest } from './Umbraco.CheckboxList.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.CheckBoxList',
		name: 'Checkbox List Property Editor UI',
		element: () => import('./property-editor-ui-checkbox-list.element.js'),
		meta: {
			label: 'Checkbox List',
			propertyEditorSchemaAlias: 'Umbraco.CheckBoxList',
			icon: 'icon-bulleted-list',
			group: 'lists',
			settings: {
				properties: [
					{
						alias: 'items',
						label: 'Add option',
						description: 'Add, remove or sort options for the list.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.MultipleTextString',
					},
				],
			},
		},
	},
	schemaManifest,
];
