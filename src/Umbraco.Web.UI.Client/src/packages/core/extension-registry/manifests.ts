import { manifests as conditionManifests } from './conditions/manifests.js';
import type { ManifestTypes } from './models/index.js';

export const manifests: Array<ManifestTypes> = [...conditionManifests];
