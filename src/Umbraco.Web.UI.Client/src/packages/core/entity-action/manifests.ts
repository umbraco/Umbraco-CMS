import { manifests as createEntityActionManifests } from './common/create/manifests.js';
import { manifests as defaultEntityActionManifests } from './default/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as duplicateEntityActionManifests } from './common/duplicate/manifests.js';
import { manifests as hasChildrenManifests } from './has-children/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...createEntityActionManifests,
	...defaultEntityActionManifests,
	...deleteEntityActionManifests,
	...duplicateEntityActionManifests,
	...hasChildrenManifests,
];
