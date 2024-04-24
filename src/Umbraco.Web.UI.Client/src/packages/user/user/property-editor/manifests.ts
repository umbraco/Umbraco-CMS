import { manifests as userPickerManifests } from './user-picker/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...userPickerManifests];
