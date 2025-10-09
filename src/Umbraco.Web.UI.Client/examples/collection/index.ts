import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dashboardManifests } from './dashboard-with-collection/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-view-with-collection/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...dashboardManifests,
	...workspaceViewManifests,
];
