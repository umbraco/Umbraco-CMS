import { manifest as schemaManifest } from './Umbraco.MultiUrlPicker.js';

export const manifests = [
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.MultiUrlPicker',
		name: 'Multi URL Picker Property Editor UI',
		element: () => import('./property-editor-ui-multi-url-picker.element.js'),
		meta: {
			label: 'Multi URL Picker',
			propertyEditorSchemaAlias: 'Umbraco.MultiUrlPicker',
			icon: 'icon-link',
			group: 'pickers',
			supportsReadOnly: true,
			settings: {
				properties: [
					{
						alias: 'overlaySize',
						label: 'Overlay Size',
						description: 'Select the width of the overlay.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.OverlaySize',
					},
					{
						alias: 'hideAnchor',
						label: 'Hide anchor/query string input',
						description: 'Selecting this hides the anchor/query string input field in the link picker overlay.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
					{
						alias: 'allowCultureSpecificDocumentLinks',
						label: 'Allow culture specific document links',
						description: 'Selecting this allows the user to pick a specific culture when linking to a document.',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
				],
			},
		},
	},
	schemaManifest,
];
