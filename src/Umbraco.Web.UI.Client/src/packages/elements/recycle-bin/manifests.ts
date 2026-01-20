import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as conditionsManifests } from './conditions/manifests.js';
import { manifests as entityActionManifests } from './entity-actions/manifests.js';
import { manifests as entityBulkActionManifests } from './entity-bulk-actions/manifests.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as rootManifests } from './root/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...conditionsManifests,
	...entityActionManifests,
	...entityBulkActionManifests,
	...folderManifests,
	...menuManifests,
	...repositoryManifests,
	...rootManifests,
	...treeManifests,
];
