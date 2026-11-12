import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as priceManifests } from './price/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...priceManifests];
