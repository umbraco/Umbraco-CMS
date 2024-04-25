import { manifests as relationTypeManifests } from './relation-type/manifests.js';
import { manifests as relationTypeRootManifests } from './relation-type-root/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...relationTypeManifests, ...relationTypeRootManifests];
