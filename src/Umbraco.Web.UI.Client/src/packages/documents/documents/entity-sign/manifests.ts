import { manifests as collectionManifests } from './has-collection/manifests.ts.js';
import { manifests as draftManifests } from './is-draft/manifests.ts.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...draftManifests];
