import { manifests as memberSectionManifests } from './section.manifests';
import { manifests as menuSectionManifests } from './menu.manifests';
import { manifests as memberGroupManifests } from './member-groups/manifests';
import { manifests as memberTypeManifests } from './member-types/manifests';
import { manifests as memberManifests } from './members/manifests';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extensions-api';

export const manifests = [
	...memberSectionManifests,
	...menuSectionManifests,
	...memberGroupManifests,
	...memberTypeManifests,
	...memberManifests,
];

export const onInit: UmbEntryPointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
