import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as extensionManifests } from './extensions/manifests.js';
import { manifests as settingsMenuManifests } from './menu.manifests.js';
import { manifests as settingsSectionManifests } from './section.manifests.js';

export const manifests = [
	...dashboardManifests,
	...extensionManifests,
	...settingsMenuManifests,
	...settingsSectionManifests,
];
