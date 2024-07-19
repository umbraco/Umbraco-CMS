import { manifests as markdownManifest } from './markdown-editor/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...markdownManifest];
