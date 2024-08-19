import { manifests as moveToManifests } from './move-to/manifests.js';
import { manifests as trashManifests } from './trash/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...moveToManifests, ...trashManifests];
