import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as workspaceActionManifests } from './workspace-action/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...modalManifests, ...workspaceActionManifests];
