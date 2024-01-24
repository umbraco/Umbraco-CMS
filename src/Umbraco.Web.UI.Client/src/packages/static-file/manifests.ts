import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';

export const manifests = [...propertyEditorManifests, ...treeManifests];
