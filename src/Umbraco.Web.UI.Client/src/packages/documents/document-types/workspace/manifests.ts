import { manifests as documentTypeManifests } from './document-type/manifests.js';
import { manifests as documentTypeRootManifests } from './document-type-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...documentTypeManifests, ...documentTypeRootManifests];
