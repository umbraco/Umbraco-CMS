import { manifests as memberTypeManifests } from './member-type/manifests.js';
import { manifests as memberTypeRootManifests } from './member-type-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...memberTypeManifests, ...memberTypeRootManifests];
