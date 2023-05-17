import { manifests as repositoryManifests } from './repository/manifests';
import { manifests as treeManifests } from './menu-item/manifests';
import { manifests as entityActions } from './entity-actions/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import { manifests as appLanguageSelect } from './app-language-select/manifests';
import { manifests as modalManifests } from './modals/manifests';

export const manifests = [
	...repositoryManifests,
	...entityActions,
	...treeManifests,
	...workspaceManifests,
	...appLanguageSelect,
	...modalManifests,
];
