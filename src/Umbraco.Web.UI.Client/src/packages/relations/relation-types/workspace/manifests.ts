import { manifests as relationTypeManifests } from './relation-type/manifests.js';
import { manifests as relationTypeRootManifests } from './relation-type-root/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...relationTypeManifests, ...relationTypeRootManifests];
