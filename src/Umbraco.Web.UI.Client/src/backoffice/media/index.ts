import { manifests as mediaSectionManifests } from './section.manifests';
import { manifests as mediaMenuManifests } from './menu.manifests';
import { manifests as mediaManifests } from './media/manifests';
import { manifests as mediaTypesManifests } from './media-types/manifests';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';

export const manifests: Array<ManifestTypes> = [
	...mediaSectionManifests,
	...mediaMenuManifests,
	...mediaManifests,
	...mediaTypesManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
