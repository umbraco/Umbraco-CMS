import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as propertyEditorManifests } from './property-editors/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...repositoryManifests, ...propertyEditorManifests];
