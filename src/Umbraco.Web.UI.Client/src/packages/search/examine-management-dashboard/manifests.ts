import { manifests as modalManifests } from './modal/manifests.js';

import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...modalManifests];
