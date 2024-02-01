import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './menu-item/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as appLanguageSelect } from './app-language-select/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';

export const manifests = [
	...repositoryManifests,
	...entityActions,
	...treeManifests,
	...workspaceManifests,
	...appLanguageSelect,
	...modalManifests,
];
