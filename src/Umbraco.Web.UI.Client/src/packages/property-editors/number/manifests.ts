import { manifests as decimalSchemaManifests } from './Umbraco.Decimal.js';
import { manifests as integerSchemaManifests } from './Umbraco.Integer.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.Decimal',
		alias: 'Umb.PropertyValuePreset.Decimal',
		name: 'Property Editor Schema Decimal Preset for Initial Values',
		api: () => import('./decimal-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Decimal',
		name: 'Decimal Property Editor UI',
		element: () => import('./property-editor-ui-number.element.js'),
		meta: {
			label: 'Decimal',
			propertyEditorSchemaAlias: 'Umbraco.Decimal',
			icon: 'icon-autofill',
			group: 'common',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'placeholder',
						label: 'Placeholder text',
						description: 'Enter the text to be displayed when the value is empty',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.TextBox',
					},
				],
				defaultData: [
					{
						alias: 'step',
						value: '0.01',
					},
				],
			},
		},
	},
	{
		type: 'propertyValuePreset',
		forPropertyEditorSchemaAlias: 'Umbraco.Integer',
		alias: 'Umb.PropertyValuePreset.Integer',
		name: 'Property Editor Schema Integer Preset for Initial Values',
		api: () => import('./integer-property-value-preset.js'),
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.Integer',
		name: 'Numeric Property Editor UI',
		element: () => import('./property-editor-ui-number.element.js'),
		meta: {
			label: 'Numeric',
			icon: 'icon-autofill',
			group: 'common',
			propertyEditorSchemaAlias: 'Umbraco.Integer',
			supportsReadOnly: true,
		},
	},
	...decimalSchemaManifests,
	...integerSchemaManifests,
];
