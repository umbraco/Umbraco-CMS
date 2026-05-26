import { manifests as defaultKindManifests } from './default/manifests.js';
import { manifests as valueTypeManifests } from './value-types/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...defaultKindManifests,
	...valueTypeManifests,
];
