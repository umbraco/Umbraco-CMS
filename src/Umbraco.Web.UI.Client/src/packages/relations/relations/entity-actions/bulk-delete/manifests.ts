import { manifest as deleteKindManifest } from './bulk-delete-with-relation.action.kind.js';
import { manifests as modalManifests } from './modal/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	deleteKindManifest,
	...modalManifests,
];
