import { manifests as memberGroupManifests } from './member-group/manifests.js';
import { manifests as memberGroupRootManifests } from './member-group-root/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...memberGroupManifests,
	...memberGroupRootManifests,
];
