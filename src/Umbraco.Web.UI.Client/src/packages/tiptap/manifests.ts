import { manifests as components } from './components/manifests.js';
import { manifests as extensions } from './extensions/manifests.js';
import { manifests as propertyEditors } from './property-editors/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...components,
	...extensions,
	...propertyEditors,
];
