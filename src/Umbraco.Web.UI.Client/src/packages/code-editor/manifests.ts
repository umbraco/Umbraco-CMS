import { manifest as propertyEditorManifest } from './property-editor/manifests.js';
import { manifests as codeEditorModalManifests } from './code-editor-modal/manifests.js';
import { manifests as valueSummaryManifests } from './property-editor/value-summary/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	propertyEditorManifest,
	...codeEditorModalManifests,
	...valueSummaryManifests,
];
