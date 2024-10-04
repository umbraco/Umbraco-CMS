import { manifests as modalManifests } from './link-picker-modal/manifests.js';
import { manifests as monacoMarkdownEditorManifests } from './monaco-markdown-editor-action/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as tinyMceManifests } from './tiny-mce-plugin/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...modalManifests,
	...monacoMarkdownEditorManifests,
	...propertyEditorManifests,
	...tinyMceManifests,
];
