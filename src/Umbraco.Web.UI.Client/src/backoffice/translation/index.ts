import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [...translationSectionManifests, ...dictionaryManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
