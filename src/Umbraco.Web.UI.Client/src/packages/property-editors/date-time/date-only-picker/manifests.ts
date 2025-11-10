import { manifest as schemaManifest } from './Umbraco.DateOnly.js';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.DateOnlyPicker',
		name: 'Date Only Picker Property Editor UI',
		element: () => import('./property-editor-ui-date-only-picker.element.js'),
		meta: {
			label: 'Date Only',
			propertyEditorSchemaAlias: 'Umbraco.DateOnly',
			icon: 'icon-calendar-alt',
			group: 'date',
			supportsReadOnly: true,
		},
	},
	{
		type: 'propertyValuePresentation',
		alias: 'Umb.PropertyValuePresentation.DateOnlyPicker',
		name: 'Date Only Picker Property Value Presentation',
		element: () => import('./property-value-presentation-date-only-picker.js'),
		propertyEditorAlias: 'Umbraco.DateOnly',
	},
	schemaManifest,
];
