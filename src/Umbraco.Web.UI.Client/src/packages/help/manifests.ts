import { manifests as headerAppManifests } from './header-app/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [...menuManifests, ...headerAppManifests];
