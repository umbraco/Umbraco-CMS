import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as itemManifests } from './item/manifests.js';
import { manifests as searchManifests } from './search/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...collectionManifests, ...itemManifests, ...searchManifests];
