import { manifest as schemaManifest } from './Umbraco.MultiUrlPicker.js';
import { manifests as modalManifests } from './link-picker-modal/manifests.js';
import { manifests as tinyMceManifests } from './tiny-mce/manifests.js';
import { manifests as monacoMarkdownEditorManifests } from './monaco-markdown-editor/manifests.js';
import type { ManifestPropertyEditorUi, ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

const manifest: ManifestPropertyEditorUi = {
	type: 'propertyEditorUi',
	alias: 'Umb.PropertyEditorUi.MultiUrlPicker',
	name: 'Multi URL Picker Property Editor UI',
	element: () => import('./property-editor-ui-multi-url-picker.element.js'),
	meta: {
		label: 'Multi URL Picker',
		propertyEditorSchemaAlias: 'Umbraco.MultiUrlPicker',
		icon: 'icon-link',
		group: 'pickers',
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
			],
		},
	},
};

export const manifests: Array<ManifestTypes> = [
	...modalManifests,
	...monacoMarkdownEditorManifests,
	...tinyMceManifests,
	manifest,
	schemaManifest,
];
