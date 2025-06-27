import { manifests as dashboardManifests } from './dashboard-with-tree/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...dashboardManifests, ...treeManifests];
