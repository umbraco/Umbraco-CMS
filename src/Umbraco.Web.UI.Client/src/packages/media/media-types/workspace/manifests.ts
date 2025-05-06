import { manifests as mediaTypeManifests } from './media-type/manifests.js';
import { manifests as mediaTypeRootManifests } from './media-type-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...mediaTypeManifests, ...mediaTypeRootManifests];
