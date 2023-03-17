import { manifests as userGroupManifests } from './user-groups/manifests';
import { manifests as userManifests } from './users/manifests';
import { manifests as userSectionManifests } from './user-section/manifests';
import { manifests as currentUserManifests } from './current-user/manifests';

import { UmbEntrypointOnInit } from '@umbraco-cms/extensions-api';
import { ManifestTypes } from '@umbraco-cms/extensions-registry';

export const manifests: Array<ManifestTypes> = [
	...userGroupManifests,
	...userManifests,
	...userSectionManifests,
	...currentUserManifests,
];

export const onInit: UmbEntrypointOnInit = (_host, extensionRegistry) => {
	extensionRegistry.registerMany(manifests);
};
