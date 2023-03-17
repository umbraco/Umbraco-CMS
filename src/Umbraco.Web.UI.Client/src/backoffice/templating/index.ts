import { manifests as menuManifests } from './menu.manifests';
import { manifests as templateManifests } from './templates/manifests';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';

export const manifests: Array<ManifestTypes> = [...menuManifests, ...templateManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
