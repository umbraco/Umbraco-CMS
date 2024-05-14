import { manifests as dashboardManifests } from './dashboards/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...dashboardManifests];
