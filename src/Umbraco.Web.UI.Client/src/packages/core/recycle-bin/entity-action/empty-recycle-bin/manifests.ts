import { manifest as emptyRecycleBinKindManifest } from './empty-recycle-bin.action.kind.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [emptyRecycleBinKindManifest];
