import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...entityActionManifests,
	...menuItemManifests,
	...repositoryManifests,
	...treeManifests,
];
