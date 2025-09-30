import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as providerManifests } from './providers/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...modalManifests, ...providerManifests];
