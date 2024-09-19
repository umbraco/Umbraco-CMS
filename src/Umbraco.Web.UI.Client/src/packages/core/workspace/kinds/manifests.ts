import { manifest as routableKindManifest } from './routable/routable-workspace.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [routableKindManifest];
