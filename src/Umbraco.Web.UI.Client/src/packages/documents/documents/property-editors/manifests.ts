import { manifest as documentPickerUI } from './document-picker/manifests.js';
import { manifest as contentPicker } from './Umbraco.ContentPicker.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [documentPickerUI, contentPicker];
