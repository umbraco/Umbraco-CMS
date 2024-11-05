import { manifests as dashboardManifests } from './document-redirect-management/manifests.js';
import { manifests as documentBlueprintManifests } from './document-blueprints/manifests.js';
import { manifests as documentManifests } from './documents/manifests.js';
import { manifests as documentTypeManifests } from './document-types/manifests.js';
import { manifests as sectionManifests } from './section/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...dashboardManifests,
	...documentBlueprintManifests,
	...documentManifests,
	...documentTypeManifests,
	...sectionManifests,
];
