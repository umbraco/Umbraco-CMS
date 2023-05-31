import { manifests as searchManifests } from '../search/manifests.js';

import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const manifests = [...searchManifests];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
