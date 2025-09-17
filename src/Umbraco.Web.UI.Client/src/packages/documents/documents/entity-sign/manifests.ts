import { manifests as collectionManifests } from './has-collection/manifests.ts';
import { manifests as draftManifests } from './is-draft/manifests.ts';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...draftManifests];
