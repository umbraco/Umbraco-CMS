import { manifests as defaultEntityActionManifests } from './default/manifests.js';
import { manifests as deleteEntityActionManifests } from './common/delete/manifests.js';
import { manifests as duplicateToEntityActionManifests } from './common/duplicate/manifests.js';
import { manifests as moveToEntityActionManifests } from './common/move-to/manifests.js';
import { manifests as sortChildrenOfEntityActionManifests } from './common/sort-children-of/manifests.js';

export const manifests = [
	...defaultEntityActionManifests,
	...deleteEntityActionManifests,
	...duplicateToEntityActionManifests,
	...moveToEntityActionManifests,
	...sortChildrenOfEntityActionManifests,
];
