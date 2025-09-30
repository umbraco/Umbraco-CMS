import { manifests as collectionMenuManifests } from './collection/manifests.js';
import { manifests as propertyEditorManifests } from './property-editor/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionMenuManifests,
	...propertyEditorManifests,
	...treeManifests,
];
