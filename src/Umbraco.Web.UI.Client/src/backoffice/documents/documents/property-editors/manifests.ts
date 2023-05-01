import { manifest as documentPickerUI } from './document-picker/manifests';
import { manifest as contentPicker } from './Umbraco.ContentPicker';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [documentPickerUI, contentPicker];
