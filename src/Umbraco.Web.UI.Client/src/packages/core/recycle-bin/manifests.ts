import { manifests as trashEntityActionManifests } from './entity-action/trash/manifests.js';
import { manifests as restoreFromRecycleBinEntityActionManifests } from './entity-action/restore-from-recycle-bin/manifests.js';
import { manifests as emptyRecycleBinEntityActionManifests } from './entity-action/empty-recycle-bin/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...conditionManifests,
	...emptyRecycleBinEntityActionManifests,
	...restoreFromRecycleBinEntityActionManifests,
	...trashEntityActionManifests,
];
