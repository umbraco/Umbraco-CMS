import { manifests as duplicateToManifests } from './duplicate-to/manifests.js';
import { manifests as moveToManifests } from './move-to/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...duplicateToManifests, ...moveToManifests];
