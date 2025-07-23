import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as propertyValueClonerManifests } from './property-value-cloner/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...workspaceManifests,
	...propertyEditorManifests,
	...propertyValueClonerManifests,
];
