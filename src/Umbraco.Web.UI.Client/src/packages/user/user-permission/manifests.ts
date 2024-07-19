import { manifests as userPermissionModalManifests } from './modals/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...userPermissionModalManifests];
