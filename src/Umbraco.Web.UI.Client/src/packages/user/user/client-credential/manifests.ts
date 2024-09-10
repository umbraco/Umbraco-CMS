import { manifests as createManifests } from './create/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [...createManifests, ...repositoryManifests];
