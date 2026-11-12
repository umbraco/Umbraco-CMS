import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as filterManifests } from './filters/manifests.js';
import { manifests as rangeKindManifests } from './range-kind/manifests.js';
import { manifests as tableViewManifests } from './table-view/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...dashboardManifests,
	...filterManifests,
	...rangeKindManifests,
	...tableViewManifests,
];
