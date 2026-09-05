import { manifest as propertyEditorManifest } from './property-editor/manifests.js';
import { manifests as codeEditorModalManifests } from './code-editor-modal/manifests.js';
import { manifests as valueSummaryManifests } from './property-editor/value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	propertyEditorManifest,
	...codeEditorModalManifests,
	...valueSummaryManifests,
];

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
