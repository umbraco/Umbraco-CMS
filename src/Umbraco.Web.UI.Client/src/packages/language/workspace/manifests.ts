import { manifests as languageManifests } from './language/manifests.js';
import { manifests as languageRootManifests } from './language-root/manifests.js';
import type { ManifestTypes } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes> = [...languageManifests, ...languageRootManifests];
