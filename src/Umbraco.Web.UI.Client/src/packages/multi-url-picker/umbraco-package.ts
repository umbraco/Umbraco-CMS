import { manifests as modalManifests } from './link-picker-modal/manifests.js';
import { manifests as documentLinkPickerModal } from './document-link-picker-modal/manifests.js';
import { manifests as monacoMarkdownEditorManifests } from './monaco-markdown-editor-action/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...modalManifests,
	...monacoMarkdownEditorManifests,
	...propertyEditorManifests,
	...documentLinkPickerModal,
];

export const name = 'Umbraco.Core.MultiUrlPicker';
export const extensions = [
	{
		name: 'Multi Url Picker Bundle',
		alias: 'Umb.Bundle.MultiUrlPicker',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
