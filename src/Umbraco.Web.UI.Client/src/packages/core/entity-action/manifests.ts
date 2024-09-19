import { manifests as defaultEntityActionManifests } from './default/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as duplicateEntityActionManifests } from './common/duplicate/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...defaultEntityActionManifests,
	...deleteEntityActionManifests,
	...duplicateEntityActionManifests,
];
