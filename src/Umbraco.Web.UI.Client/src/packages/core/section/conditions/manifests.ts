import { manifests as sectionAliasManifests } from './section-alias/manifests.js';
import { manifests as sectionUserPermissionManifests } from './section-user-permission/manifests.js';
import { manifests as sectionUserNoPermissionManifests } from './section-user-no-permission/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...sectionAliasManifests,
	...sectionUserPermissionManifests,
	...sectionUserNoPermissionManifests,
];
