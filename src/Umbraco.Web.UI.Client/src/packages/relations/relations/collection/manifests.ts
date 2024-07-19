import { manifests as collectionRepositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...collectionRepositoryManifests];
