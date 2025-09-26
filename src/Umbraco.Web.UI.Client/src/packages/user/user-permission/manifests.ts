import { manifests as userPermissionModalManifests } from './modals/manifests.js';
import { manifests as fallbackConditionPermission } from './fallback-permission-condition/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...userPermissionModalManifests, ...fallbackConditionPermission];
