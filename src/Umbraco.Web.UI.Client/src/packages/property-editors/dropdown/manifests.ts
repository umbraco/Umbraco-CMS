import { manifest as schemaManifest } from './Umbraco.Dropdown.Flexible.js';
import { manifest as singleSchemaManifest } from './Umbraco.SingleDropDown.js';

const keywords = ['select', 'dropdown', 'choice', 'option', 'list'];

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Dropdown',
		name: 'Dropdown Property Editor UI',
		element: () => import('./property-editor-ui-dropdown.element.js'),
		meta: {
			label: 'Dropdown',
			propertyEditorSchemaAlias: 'Umbraco.DropDown.Flexible',
			icon: 'icon-list',
			group: '#propertyEditorUIGroups_lists',
			keywords: [...keywords, 'multiple', 'several'],
			supportsReadOnly: true,
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.SingleDropdown',
		name: 'Single Dropdown Property Editor UI',
		element: () => import('./property-editor-ui-single-dropdown.element.js'),
		meta: {
			label: 'Single Dropdown',
			propertyEditorSchemaAlias: 'Umbraco.SingleDropDown',
			icon: 'icon-list',
			group: '#propertyEditorUIGroups_lists',
			keywords: [...keywords, 'single', 'one'],
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'placeholder',
						label: '#general_placeholder',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
			},
		},
	},
	schemaManifest,
	singleSchemaManifest,
];
