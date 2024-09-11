import { manifests as userManifests } from './user/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...userManifests];
