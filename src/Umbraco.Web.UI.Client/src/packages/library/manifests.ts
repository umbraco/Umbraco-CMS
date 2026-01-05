import { manifests as sectionManifests } from './section/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as dashboardManifests } from './dashboard/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...sectionManifests, ...menuManifests, ...dashboardManifests];
