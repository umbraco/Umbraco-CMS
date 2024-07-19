import { manifests as propertyEditors } from './property-editors/manifests.js';
import { manifests as plugins } from './plugins/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...propertyEditors, ...plugins, ...modalManifests];
