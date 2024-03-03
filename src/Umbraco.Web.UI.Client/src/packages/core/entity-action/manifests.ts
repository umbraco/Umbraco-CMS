import { manifests as copyEntityActionManifests } from './common/duplicate/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as moveEntityActionManifests } from './common/move/manifests.js';
import { manifests as renameEntityActionManifests } from './common/rename/manifests.js';
import { manifests as defaultEntityActionManifests } from './common/default/manifests.js';

export const manifests = [
	...copyEntityActionManifests,
	...deleteEntityActionManifests,
	...moveEntityActionManifests,
	...renameEntityActionManifests,
	...defaultEntityActionManifests,
];
