import { manifest as userPermissionConditionManifest } from './conditions/user-permission.condition.js';
import { manifests as userPermissionModalManifests } from './modals/manifests.js';

export const manifests = [userPermissionConditionManifest, ...userPermissionModalManifests];
