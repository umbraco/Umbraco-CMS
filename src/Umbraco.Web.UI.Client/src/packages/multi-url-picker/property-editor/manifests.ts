import { manifest as schemaManifest } from './Umbraco.MultiUrlPicker.js';
import { manifest as singleSchemaManifest } from './Umbraco.UrlPicker.Single.js';
import { manifests as valueSummaryManifests } from './value-summary/manifests.js';

const settingsProperties = [
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
];

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
			group: '#propertyEditorUIGroups_pickers',
			keywords: ['url', 'link', 'cta', 'links', 'multiple', 'several'],
			supportsReadOnly: true,
			settings: {
				properties: settingsProperties,
			},
		},
	},
	{
		type: 'propertyEditorUi',
		alias: 'Umb.PropertyEditorUi.UrlPicker.Single',
		name: 'Single URL Picker Property Editor UI',
		element: () => import('./property-editor-ui-single-url-picker.element.js'),
		meta: {
			label: 'Single URL Picker',
			propertyEditorSchemaAlias: 'Umbraco.UrlPicker.Single',
			icon: 'icon-link',
			group: '#propertyEditorUIGroups_pickers',
			keywords: ['url', 'link', 'cta', 'single', 'one'],
			supportsReadOnly: true,
			settings: {
				properties: settingsProperties,
			},
		},
	},
	schemaManifest,
	singleSchemaManifest,
	...valueSummaryManifests,
];
