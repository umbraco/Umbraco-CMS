import { manifest as entityReferencesKindManifest } from './entity-references-workspace-view-info.kind.js';

import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [entityReferencesKindManifest];
