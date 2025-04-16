import { manifests as treeManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...treeManifests, ...workspaceManifests];
