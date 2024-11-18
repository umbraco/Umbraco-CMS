import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { manifests as defaultKindManifests } from './default/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [...defaultKindManifests];
