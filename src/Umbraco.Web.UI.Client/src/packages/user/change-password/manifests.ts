import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...entityActionManifests, ...modalManifests];
