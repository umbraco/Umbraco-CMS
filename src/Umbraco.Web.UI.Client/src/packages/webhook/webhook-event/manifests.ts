import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...modalManifests, ...repositoryManifests];
