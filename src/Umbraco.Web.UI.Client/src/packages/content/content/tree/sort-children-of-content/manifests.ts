import { manifest as sortChildrenOfContentKindManifest } from './sort-children-of-content.action.kind.js';
import { manifests as modalManifests } from './modal/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	sortChildrenOfContentKindManifest,
	...modalManifests,
];
