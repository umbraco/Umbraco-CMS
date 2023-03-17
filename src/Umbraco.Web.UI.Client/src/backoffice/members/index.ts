import { manifests as memberSectionManifests } from './section.manifests';
import { manifests as menuSectionManifests } from './menu.manifests';
import { manifests as memberGroupManifests } from './member-groups/manifests';
import { manifests as memberTypeManifests } from './member-types/manifests';
import { manifests as memberManifests } from './members/manifests';
import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...memberSectionManifests,
	...menuSectionManifests,
	...memberGroupManifests,
	...memberTypeManifests,
	...memberManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
