import { manifests as webhookManifests } from './webhook/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...webhookManifests];
