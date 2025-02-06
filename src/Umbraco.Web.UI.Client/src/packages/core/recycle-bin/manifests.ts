import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as emptyRecycleBinEntityActionManifests } from './entity-action/empty-recycle-bin/manifests.js';
import { manifests as restoreFromRecycleBinEntityActionManifests } from './entity-action/restore-from-recycle-bin/manifests.js';
import { manifests as trashEntityActionManifests } from './entity-action/trash/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...conditionManifests,
	...emptyRecycleBinEntityActionManifests,
	...restoreFromRecycleBinEntityActionManifests,
	...trashEntityActionManifests,
	...treeManifests,
];
