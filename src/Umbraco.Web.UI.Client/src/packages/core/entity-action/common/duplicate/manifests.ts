import { manifests as duplicateManifests } from './duplicate/manifests.js';
import { manifests as duplicateToManifests } from './duplicate-to/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...duplicateManifests,
	...duplicateToManifests,
];
