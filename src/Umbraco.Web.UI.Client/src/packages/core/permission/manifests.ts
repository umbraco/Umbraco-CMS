import { manifests as conditionManifests } from './condition/manifests.js';
import { manifests as globalPermissionManifests } from './global-permission/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...conditionManifests, ...globalPermissionManifests];
