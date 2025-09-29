import { manifests as collectionManifests } from './has-collection/manifests.js';
import { manifests as draftManifests } from './is-draft/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...draftManifests];
