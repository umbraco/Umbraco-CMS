import { manifest as schemaManifest } from './Umbraco.MultiUrlPicker.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

const propertyEditorUiAlias = 'Umb.PropertyEditorUi.MultiUrlPicker';

export const manifests = [
	{
		type: 'propertyEditorUi',
		alias: propertyEditorUiAlias,
		name: 'Multi URL Picker Property Editor UI',
		element: () => import('./property-editor-ui-multi-url-picker.element.js'),
		meta: {
			supportsVariantChange: true,
			label: 'Multi URL Picker',
			propertyEditorSchemaAlias: 'Umbraco.MultiUrlPicker',
			icon: 'icon-link',
			group: '#propertyEditorUIGroups_pickers',
			keywords: ['url', 'link', 'cta', 'links'],
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
						label: '#linkPicker_configCultureSpecificDocumentLinksLabel',
						description: '{#linkPicker_configCultureSpecificDocumentLinksDescription}',
						propertyEditorUiAlias: 'Umb.PropertyEditorUi.Toggle',
					},
				],
			},
		},
	},
	schemaManifest,
	...valueSummaryManifests,
	{
		type: 'propertyAction',
		kind: 'clear',
		alias: 'Umb.PropertyAction.MultiUrlPicker.Clear',
		name: 'Clear Multi URL Picker Property Action',
		forPropertyEditorUis: [propertyEditorUiAlias],
	},
];
