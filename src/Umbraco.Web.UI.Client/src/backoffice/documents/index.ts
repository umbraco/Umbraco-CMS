import { manifests as dashboardManifests } from './dashboards/manifests';
import { manifests as contentSectionManifests } from './section.manifests';
import { manifests as contentMenuManifest } from './menu.manifests';
import { manifests as documentBlueprintManifests } from './document-blueprints/manifests';
import { manifests as documentTypeManifests } from './document-types/manifests';
import { manifests as documentManifests } from './documents/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...dashboardManifests,
	...contentSectionManifests,
	...contentMenuManifest,
	...documentBlueprintManifests,
	...documentTypeManifests,
	...documentManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
