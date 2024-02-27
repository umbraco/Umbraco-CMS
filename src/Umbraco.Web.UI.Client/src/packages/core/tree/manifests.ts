import { manifests as folderManifests } from './folder/manifests.js';
import { manifests as defaultTreeItemManifests } from './tree-item/tree-item-default/manifests.js';

export const manifests = [...folderManifests, ...defaultTreeItemManifests];
