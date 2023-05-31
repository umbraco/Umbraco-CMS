import type { ManifestPropertyEditorUI } from '@umbraco-cms/backoffice/extension-registry';

// TODO: we don't really want this config value to be changed from the UI. We need a way to handle hidden config properties.
const inputTypeConfig = {
	alias: 'inputType',
	label: 'Input type',
	description: 'Select input type',
	propertyEditorUI: 'Umb.PropertyEditorUi.Dropdown',
};

export const manifests: Array<ManifestPropertyEditorUI> = [
	{
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUi.TextBox',
		name: 'Text Box Property Editor UI',
		loader: () => import('./property-editor-ui-text-box.element.js'),
		meta: {
			label: 'Text Box',
			propertyEditorModel: 'Umbraco.TextBox',
			icon: 'umb:autofill',
			group: 'common',
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
		type: 'propertyEditorUI',
		alias: 'Umb.PropertyEditorUi.Email',
		name: 'Email Property Editor UI',
		loader: () => import('./property-editor-ui-text-box.element.js'),
		meta: {
			label: 'Email',
			propertyEditorModel: 'Umbraco.EmailAddress',
			icon: 'umb:message',
			group: 'common',
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
];
