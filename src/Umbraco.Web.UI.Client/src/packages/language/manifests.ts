import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as appLanguageSelect } from './app-language-select/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as globalContextManifests } from './global-contexts/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [
	...repositoryManifests,
	...entityActions,
	...menuManifests,
	...workspaceManifests,
	...appLanguageSelect,
	...modalManifests,
	...collectionManifests,
	...globalContextManifests,
];
