import { manifests as clipboardManifests } from './clipboard/manifests.js';
import { manifests as componentManifests } from './components/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as propertyValueClonerManifests } from './property-value-cloner/manifests.js';
import { manifests as validationManifests } from './validation/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...clipboardManifests,
	...componentManifests,
	...propertyEditorManifests,
	...propertyValueClonerManifests,
	...validationManifests,
	...workspaceManifests,
];
