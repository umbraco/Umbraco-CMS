import { manifests as defaultEntityBulkActionManifests } from './default/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...defaultEntityBulkActionManifests,
];
