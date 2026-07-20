import { UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST } from './default/index.js';
import { UMB_PROPERTY_ACTION_CLEAR_KIND_MANIFEST } from './clear/index.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	UMB_PROPERTY_ACTION_DEFAULT_KIND_MANIFEST,
	UMB_PROPERTY_ACTION_CLEAR_KIND_MANIFEST,
];
