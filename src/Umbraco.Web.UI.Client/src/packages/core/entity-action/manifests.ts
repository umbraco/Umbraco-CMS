import { manifests as defaultEntityActionManifests } from './default/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as duplicateEntityActionManifests } from './common/duplicate/manifests.js';
import { manifests as moveToEntityActionManifests } from './common/move/manifests.js';
import { manifests as sortChildrenOfEntityActionManifests } from './common/sort-children-of/manifests.js';

export const manifests = [
	...defaultEntityActionManifests,
	...deleteEntityActionManifests,
	...duplicateEntityActionManifests,
	...moveToEntityActionManifests,
	...sortChildrenOfEntityActionManifests,
];
