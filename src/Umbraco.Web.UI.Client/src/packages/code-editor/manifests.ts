import { manifest as propertyEditorManifest } from './property-editor/manifests.js';
import { manifests as codeEditorModalManifests } from './code-editor-modal/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [propertyEditorManifest, ...codeEditorModalManifests];
