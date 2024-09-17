import { manifests as entityActionManifests } from './entity-action/manifests.js';
import { manifests as modalManifests } from './modal/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...entityActionManifests, ...modalManifests, ...repositoryManifests];
