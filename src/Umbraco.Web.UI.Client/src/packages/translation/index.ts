import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import { manifests as modalManifests } from './modals/manifests';

import { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const manifests = [...modalManifests, ...translationSectionManifests, ...dictionaryManifests];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
