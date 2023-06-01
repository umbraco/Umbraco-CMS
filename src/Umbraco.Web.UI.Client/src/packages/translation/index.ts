import { manifests as translationSectionManifests } from './section.manifest.js';
import { manifests as dictionaryManifests } from './dictionary/manifests.js';

import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const manifests = [...translationSectionManifests, ...dictionaryManifests];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
