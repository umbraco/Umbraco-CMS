import { manifests as classicManifests } from './classic/manifests.js';
import { manifests as cardManifests } from './card/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...classicManifests, ...cardManifests];
