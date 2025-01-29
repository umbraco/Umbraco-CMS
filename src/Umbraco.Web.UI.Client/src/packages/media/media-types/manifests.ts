import { manifests as entityActionsManifests } from './entity-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import { manifests as propertyEditorUiManifests } from './property-editors/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...entityActionsManifests,
	...menuManifests,
	...repositoryManifests,
	...treeManifests,
	...workspaceManifests,
	...propertyEditorUiManifests,
	...searchManifests,
];
