import { manifests as entityUserPermissionManifests } from './entity-user-permission/manifests.js';
import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as localizationManifests } from './localization/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...entityUserPermissionManifests,
	...entityActionManifests,
	...localizationManifests,
];
