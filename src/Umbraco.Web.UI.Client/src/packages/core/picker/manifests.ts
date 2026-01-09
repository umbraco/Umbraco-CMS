import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...conditionManifests,
	...searchManifests,
];
