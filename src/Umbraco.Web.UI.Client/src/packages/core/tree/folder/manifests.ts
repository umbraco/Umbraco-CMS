import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';

export const manifests: Array<ManifestTypes> = [...modalManifests, ...entityActionManifests];
