import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...collectionManifests,
	...dashboardManifests,
	...entityActionManifests,
	...menuManifests,
	...repositoryManifests,
	...sectionManifests,
	...treeManifests,
	...workspaceManifests,
];
