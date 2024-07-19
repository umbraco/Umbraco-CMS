import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as relationManifests } from './relations/manifests.js';
import { manifests as relationTypeManifests } from './relation-types/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...relationTypeManifests, ...relationManifests, ...menuManifests];
