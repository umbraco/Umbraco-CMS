import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [...translationSectionManifests, ...dictionaryManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
