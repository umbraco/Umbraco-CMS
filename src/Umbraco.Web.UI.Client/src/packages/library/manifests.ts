import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as sectionManifests } from './section/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...menuManifests, ...sectionManifests];
