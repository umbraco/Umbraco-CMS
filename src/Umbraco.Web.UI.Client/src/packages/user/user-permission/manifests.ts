import { manifests as uiUserPermissionManifests } from './ui-user-permission/manifests.js';
import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...uiUserPermissionManifests, ...userPermissionModalManifests];
