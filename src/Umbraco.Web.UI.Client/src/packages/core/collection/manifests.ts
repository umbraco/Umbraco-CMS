import type { UmbExtensionManifestKind } from '../extension-registry/registry.js';
import { manifest as collectionAliasCondition } from './collection-alias.manifest.js';
import { manifest as collectionBulkActionPermissionCondition } from './collection-bulk-action-permission.manifest.js';
import { manifests as workspaceViewManifests } from './workspace-view/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...workspaceViewManifests,
	collectionAliasCondition,
	collectionBulkActionPermissionCondition,
];
