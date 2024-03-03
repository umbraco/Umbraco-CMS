import { manifests as copyEntityActionManifests } from './common/duplicate/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as moveEntityActionManifests } from './common/move/manifests.js';
import { manifests as renameEntityActionManifests } from './common/rename/manifests.js';

export const manifests = [
	...copyEntityActionManifests,
	...deleteEntityActionManifests,
	...moveEntityActionManifests,
	...renameEntityActionManifests,
];
