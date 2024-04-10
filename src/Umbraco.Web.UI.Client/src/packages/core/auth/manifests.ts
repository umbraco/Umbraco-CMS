import type { ManifestTypes } from '../extension-registry/models/index.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as providerManifests } from './providers/manifests.js';

export const manifests: Array<ManifestTypes> = [...modalManifests, ...providerManifests];
