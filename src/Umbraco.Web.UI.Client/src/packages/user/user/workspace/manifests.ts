import { manifests as userManifests } from './user/manifests.js';
import { manifests as userRootManifests } from './user-root/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [...userManifests, ...userRootManifests];
