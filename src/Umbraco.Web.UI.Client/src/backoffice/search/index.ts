import { manifests as searchManifests } from '../search/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [...searchManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
