import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as reloadEntityActionManifests } from './reload-entity-action/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...reloadEntityActionManifests];
