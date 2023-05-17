import { manifests as mediaSectionManifests } from './section.manifests';
import { manifests as mediaMenuManifests } from './menu.manifests';
import { manifests as mediaManifests } from './media/manifests';
import { manifests as mediaTypesManifests } from './media-types/manifests';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

export const manifests = [...mediaSectionManifests, ...mediaMenuManifests, ...mediaManifests, ...mediaTypesManifests];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
