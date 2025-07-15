import { manifest as textBoxSchemaManifest } from './Umbraco.TextBox.js';
import { manifest as emailSchemaManifest } from './Umbraco.EmailAddress.js';

// TODO: we don't really want this config value to be changed from the UI. We need a way to handle hidden config properties.
const inputTypeConfig = {
	alias: 'inputType',
	label: 'Input type',
	description: 'Predefined input type',
	propertyEditorUiAlias: 'Umb.PropertyEditorUi.Label',
};

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.TextBox',
		name: 'Text Box Property Editor UI',
		element: () => import('./property-editor-ui-text-box.element.js'),
		meta: {
			label: 'Text Box',
			propertyEditorSchemaAlias: 'Umbraco.TextBox',
			icon: 'icon-autofill',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [inputTypeConfig],
				defaultData: [
					{
						alias: 'inputType',
						value: 'text',
					},
				],
			},
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.EmailAddress',
		name: 'Email Property Editor UI',
		element: () => import('./property-editor-ui-text-box.element.js'),
		meta: {
			label: 'Email',
			propertyEditorSchemaAlias: 'Umbraco.EmailAddress',
			icon: 'icon-message',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [inputTypeConfig],
				defaultData: [
					{
						alias: 'inputType',
						value: 'email',
					},
				],
			},
		},
	},
	textBoxSchemaManifest,
	emailSchemaManifest,
];
