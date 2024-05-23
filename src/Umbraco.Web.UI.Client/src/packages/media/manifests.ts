import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaSectionManifests } from './section.manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';
import { manifests as imagingManifests } from './imaging/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...mediaSectionManifests,
	...mediaManifests,
	...mediaTypesManifests,
	...imagingManifests,
];
