import { manifest as defaultKind } from './default.preview-option.kind.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	defaultKind,
];
