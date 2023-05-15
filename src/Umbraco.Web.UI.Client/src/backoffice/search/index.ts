import { manifests as searchManifests } from '../search/manifests';

import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [...searchManifests];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
