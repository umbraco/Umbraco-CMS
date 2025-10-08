import { manifests as communityManifests } from './community/manifests.js';
import { manifests as documentationManifests } from './documentation/manifests.js';
import { manifests as supportManifests } from './support/manifests.js';
import { manifests as trainingManifests } from './certification/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...communityManifests,
	...documentationManifests,
	...supportManifests,
	...trainingManifests,
];
