import { manifests as createManifests } from './create/manifests.js';
import { manifests as moveManifests } from './move/manifests.js';
import { manifests as reloadManifests } from './reload/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...createManifests, ...moveManifests, ...reloadManifests];
