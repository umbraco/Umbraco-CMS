import { manifests as trashEntityActionManifests } from '../recycle-bin/entity-action/trash/manifests.js';
import { manifests as restoreFromRecycleBinEntityActionManifests } from '../recycle-bin/entity-action/restore-from-recycle-bin/manifests.js';
import { manifests as emptyRecycleBinEntityActionManifests } from '../recycle-bin/entity-action/empty-recycle-bin/manifests.js';

export const manifests = [
	...trashEntityActionManifests,
	...restoreFromRecycleBinEntityActionManifests,
	...emptyRecycleBinEntityActionManifests,
];
