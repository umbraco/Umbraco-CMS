import { manifests as mediaManifests } from './media/manifests.js';
import { manifests as mediaSectionManifests } from './media-section/manifests.js';
import { manifests as mediaTypesManifests } from './media-types/manifests.js';
import { manifests as imagingManifests } from './imaging/manifests.js';
import { manifests as dropzoneManifests } from './dropzone/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...mediaSectionManifests,
	...mediaManifests,
	...mediaTypesManifests,
	...imagingManifests,
	...dropzoneManifests,
];
