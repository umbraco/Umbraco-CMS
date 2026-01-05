import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...menuManifests, ...treeManifests];
