import { manifests as documentPickerManifests } from './document-picker/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...documentPickerManifests];
