import { manifests as translationSectionManifests } from './section.manifest';
import { manifests as dictionaryManifests } from './dictionary/manifests';
import type { ManifestTypes } from '@umbraco-cms/models';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';

export const manifests: Array<ManifestTypes> = [...translationSectionManifests, ...dictionaryManifests];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
