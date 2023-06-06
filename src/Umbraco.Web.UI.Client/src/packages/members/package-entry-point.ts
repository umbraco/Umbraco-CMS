import { manifests as memberSectionManifests } from './section.manifests.js';
import { manifests as menuSectionManifests } from './menu.manifests.js';
import { manifests as memberGroupManifests } from './member-groups/manifests.js';
import { manifests as memberTypeManifests } from './member-types/manifests.js';
import { manifests as memberManifests } from './members/manifests.js';
import type { UmbEntryPointOnInit } from '@umbraco-cms/backoffice/extension-api';

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
