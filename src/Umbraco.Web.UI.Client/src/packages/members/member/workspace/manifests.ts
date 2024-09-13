import { manifests as memberManifests } from './member/manifests.js';
import { manifests as memberRootManifests } from './member-root/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [...memberManifests, ...memberRootManifests];
