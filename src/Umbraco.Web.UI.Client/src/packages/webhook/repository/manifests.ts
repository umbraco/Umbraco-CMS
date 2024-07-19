import { manifests as detailManifests } from './detail/manifests.js';
import { manifests as eventManifests } from './event/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...detailManifests, ...eventManifests, ...itemManifests];
