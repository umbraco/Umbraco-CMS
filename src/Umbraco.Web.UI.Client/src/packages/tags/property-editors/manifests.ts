import { manifests as tagsUI } from './tags/manifests';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...tagsUI];
