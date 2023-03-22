import { manifests as menuManifests } from './menu.manifests';
import { manifests as templateManifests } from './templates/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [...menuManifests, ...templateManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
