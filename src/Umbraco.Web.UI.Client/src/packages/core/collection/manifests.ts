import { manifest as collectionAliasCondition } from './collection-alias.manifest.js';
import { manifest as collectionBulkActionPermissionCondition } from './collection-bulk-action-permission.manifest.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [collectionAliasCondition, collectionBulkActionPermissionCondition];
