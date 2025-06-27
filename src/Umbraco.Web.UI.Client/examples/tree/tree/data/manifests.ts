import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as storeManifests } from './store/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...repositoryManifests, ...storeManifests];
