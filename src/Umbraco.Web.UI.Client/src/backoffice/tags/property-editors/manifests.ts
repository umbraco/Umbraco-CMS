import { manifests as tagsUI } from './tags/manifests';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extensions-registry';

export const manifests: Array<ManifestTypes> = [...tagsUI];
