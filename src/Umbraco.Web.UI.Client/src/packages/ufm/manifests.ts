import { manifest as ufmContext } from './contexts/manifest.js';
import { manifests as ufmComponents } from './components/manifests.js';
import { manifests as ufmFilters } from './filters/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [ufmContext, ...ufmComponents, ...ufmFilters];
