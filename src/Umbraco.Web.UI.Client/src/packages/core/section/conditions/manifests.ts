import { manifests as sectionAliasManifests } from './section-alias/manifests.js';
import { manifests as sectionUserPermissionManifests } from './section-user-permission/manifests.js';

export const manifests: Array<UmbExtensionManifest> = [...sectionAliasManifests, ...sectionUserPermissionManifests];
