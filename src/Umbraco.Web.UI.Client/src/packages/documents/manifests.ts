import { manifests as dashboardManifests } from './dashboards/manifests.js';
import { manifests as contentSectionManifests } from './section.manifests.js';
import { manifests as contentMenuManifest } from './menu.manifests.js';
import { manifests as documentBlueprintManifests } from './document-blueprints/manifests.js';
import { manifests as documentTypeManifests } from './document-types/manifests.js';
import { manifests as documentManifests } from './documents/manifests.js';

export const manifests = [
	...dashboardManifests,
	...contentSectionManifests,
	...contentMenuManifest,
	...documentBlueprintManifests,
	...documentTypeManifests,
	...documentManifests,
];
