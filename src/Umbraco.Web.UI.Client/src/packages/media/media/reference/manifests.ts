import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as infoAppManifests } from './info-app/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...repositoryManifests, ...infoAppManifests];
