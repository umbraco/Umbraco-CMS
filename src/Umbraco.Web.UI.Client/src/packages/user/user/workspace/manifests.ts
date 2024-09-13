import { manifests as userManifests } from './user/manifests.js';
import { manifests as userRootManifests } from './user-root/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [...userManifests, ...userRootManifests];
