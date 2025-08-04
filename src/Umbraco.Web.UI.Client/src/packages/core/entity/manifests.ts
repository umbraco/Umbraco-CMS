import { manifests as entityTypeManifests } from './entity-type/manifests.js';
import { manifests as entityUniqueManifests } from './entity-unique/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...entityTypeManifests, ...entityUniqueManifests];
