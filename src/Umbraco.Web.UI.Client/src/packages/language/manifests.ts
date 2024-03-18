import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as navigationManifests } from './navigation/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as appLanguageSelect } from './app-language-select/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as globalContextManifests } from './global-contexts/manifests.js';

export const manifests = [
	...repositoryManifests,
	...entityActions,
	...navigationManifests,
	...workspaceManifests,
	...appLanguageSelect,
	...modalManifests,
	...collectionManifests,
	...globalContextManifests,
];
