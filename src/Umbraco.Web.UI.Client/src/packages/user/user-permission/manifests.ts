import { manifests as contextualUserPermissionManifests } from './contextual-user-permission/manifests.js';
import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [
	...contextualUserPermissionManifests,
	...userPermissionModalManifests,
];
