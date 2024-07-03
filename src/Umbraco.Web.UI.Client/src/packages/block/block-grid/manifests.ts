import { manifests as componentManifests } from './components/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...workspaceManifests,
	...propertyEditorManifests,
	...componentManifests,
];
