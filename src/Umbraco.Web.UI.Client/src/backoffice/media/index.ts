import { manifests as mediaSectionManifests } from './section.manifests';
import { manifests as mediaMenuManifests } from './menu.manifests';
import { manifests as mediaManifests } from './media/manifests';
import { manifests as mediaTypesManifests } from './media-types/manifests';
import type { UmbEntrypointOnInit } from '@umbraco-cms/backoffice/extensions-api';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...mediaSectionManifests,
	...mediaMenuManifests,
	...mediaManifests,
	...mediaTypesManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
