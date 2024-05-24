import { manifests as reloadTreeItemChildrenManifests } from './reload-tree-item-children/manifests.js';
import { manifests as moveManifests } from './move/manifests.js';
import { manifests as sortChildrenOfManifests } from './sort-children-of/manifests.js';

export const manifests = [...reloadTreeItemChildrenManifests, ...moveManifests, ...sortChildrenOfManifests];
