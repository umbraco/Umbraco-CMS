import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as duplicateToManifests } from './duplicate-to/manifests.js';

export const manifests: Array<ManifestTypes> = [...duplicateManifests, ...duplicateToManifests];
