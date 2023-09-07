import { manifests as cultureManifests } from './cultures/manifests.js';
import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as dataTypeManifests } from './data-types/manifests.js';
import { manifests as extensionManifests } from './extensions/manifests.js';
import { manifests as languageManifests } from './languages/manifests.js';
import { manifests as logviewerManifests } from '../log-viewer/manifests.js';
import { manifests as relationTypeManifests } from './relation-types/manifests.js';
import { manifests as settingsMenuManifests } from './menu.manifests.js';
import { manifests as settingsSectionManifests } from './section.manifests.js';

export const manifests = [
	...cultureManifests,
	...dashboardManifests,
	...dataTypeManifests,
	...extensionManifests,
	...languageManifests,
	...logviewerManifests,
	...relationTypeManifests,
	...settingsMenuManifests,
	...settingsSectionManifests,
];
