import { manifests as searchManifests } from '../search/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [...searchManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
