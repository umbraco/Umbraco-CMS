import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as settingsMenuManifests } from './menu.manifests.js';
import { manifests as settingsSectionManifests } from './section.manifests.js';

export const manifests = [...dashboardManifests, ...settingsMenuManifests, ...settingsSectionManifests];
