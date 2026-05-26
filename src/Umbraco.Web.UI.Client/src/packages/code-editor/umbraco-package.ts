import { manifest as propertyEditorManifest } from './property-editor/manifests.js';
import { manifests as codeEditorModalManifests } from './code-editor-modal/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [propertyEditorManifest, ...codeEditorModalManifests];

export const name = 'Umbraco.CodeEditor';
export const extensions = [
	{
		name: 'Umbraco Code Editor Bundle',
		alias: 'Umb.Bundle.UmbracoCodeEditor',
		type: 'bundle',
		js: {
			manifests,
		},
	},
];
