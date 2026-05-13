import { UMB_BLOCK_ACTION_DEFAULT_KIND_MANIFEST } from './default.action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_BLOCK_ACTION_DEFAULT_KIND_MANIFEST,
];
