import { manifest as schemaManifest } from './Umbraco.CheckboxList.js';

export const manifests: Array<UmbExtensionManifest> = [
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
			supportsReadOnly: true,
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
