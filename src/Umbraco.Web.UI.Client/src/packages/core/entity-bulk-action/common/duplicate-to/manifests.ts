import { manifest as duplicateToKindManifest } from './duplicate-to.action.kind.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [duplicateToKindManifest];
