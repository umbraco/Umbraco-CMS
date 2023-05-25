import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as contentSectionManifests } from './section.manifests';
import { manifests as contentMenuManifest } from './menu.manifests';
import { manifests as documentBlueprintManifests } from './document-blueprints/manifests';
import { manifests as documentTypeManifests } from './document-types/manifests';
import { manifests as documentManifests } from './documents/manifests';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

import './document-types';
import './documents';

export const manifests = [
	...dashboardManifests,
	...contentSectionManifests,
	...contentMenuManifest,
	...documentBlueprintManifests,
	...documentTypeManifests,
	...documentManifests,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
