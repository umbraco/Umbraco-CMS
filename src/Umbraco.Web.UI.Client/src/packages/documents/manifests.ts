import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as documentBlueprintManifests } from './document-blueprints/manifests.js';
import { manifests as documentManifests } from './documents/manifests.js';
import { manifests as documentTypeManifests } from './document-types/manifests.js';
import { manifests as sectionManifests } from './section.manifests.js';

export const manifests = [
	...dashboardManifests,
	...documentBlueprintManifests,
	...documentManifests,
	...documentTypeManifests,
	...sectionManifests,
];
