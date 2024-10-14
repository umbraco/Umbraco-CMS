import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchProviderManifests } from './search/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...collectionManifests,
	...entityActions,
	...menuManifests,
	...modalManifests,
	...repositoryManifests,
	...searchProviderManifests,
	...treeManifests,
	...workspaceManifests,
];
