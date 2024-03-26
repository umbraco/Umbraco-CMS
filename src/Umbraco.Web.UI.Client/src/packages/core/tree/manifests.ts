import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as defaultTreeItemManifests } from './tree-item/tree-item-default/manifests.js';
import { manifests as defaultTreeManifests } from './default/manifests.js';
import { manifests as treePickerManifests } from './tree-picker/manifests.js';
import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';

export const manifests = [
	...defaultTreeManifests,
	...folderManifests,
	...defaultTreeItemManifests,
	...treePickerManifests,
	...reloadTreeItemChildrenManifests,
];
