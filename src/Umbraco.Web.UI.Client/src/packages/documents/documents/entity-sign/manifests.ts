import { manifests as collectionManifests } from './has-collection/manifests.js';
import { manifests as draftManifests } from './is-draft/manifests.js';
import { manifests as protectedManifest } from './is-protected/manifest.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...draftManifests, protectedManifest];
