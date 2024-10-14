import { manifests as userManifests } from './user/manifests.js';
import { manifests as userRootManifests } from './user-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...userManifests, ...userRootManifests];
