import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifest as kindManifest } from './tree-item-card-default.kind.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [kindManifest];
