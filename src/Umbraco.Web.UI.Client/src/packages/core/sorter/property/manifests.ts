import { manifests as actionManifests } from './actions/manifests.js';
import { manifests as contextManifests } from './context/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...actionManifests,
	...contextManifests,
];
