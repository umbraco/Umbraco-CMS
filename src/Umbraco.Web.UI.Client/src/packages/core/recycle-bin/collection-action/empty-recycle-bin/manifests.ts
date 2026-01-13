import { manifest as emptyRecycleBinKindManifest } from './empty-recycle-bin.collection-action.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [emptyRecycleBinKindManifest];
