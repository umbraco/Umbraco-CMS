import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-view/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...workspaceViewManifests];
