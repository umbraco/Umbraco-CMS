import { manifests as modalManifests } from './modals/manifests.js';
import { manifest as propertyEditorManifest } from './property-editor/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...modalManifests, propertyEditorManifest];
