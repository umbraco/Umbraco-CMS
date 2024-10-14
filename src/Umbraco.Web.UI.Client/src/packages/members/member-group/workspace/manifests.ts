import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberGroupRootManifests } from './member-group-root/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...memberGroupManifests,
	...memberGroupRootManifests,
];
