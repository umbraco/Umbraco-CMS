import { manifests as extensions } from './extensions/manifests.js';
import { manifests as propertyEditors } from './property-editors/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...extensions, ...propertyEditors];
