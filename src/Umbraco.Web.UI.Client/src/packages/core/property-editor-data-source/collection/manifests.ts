import { manifests as dataManifests } from './data/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...dataManifests, ...menuManifests];
