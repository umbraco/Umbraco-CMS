import { manifests as actionManifests } from './action/manifests.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...actionManifests,
	...conditionManifests,
	...modalManifests,
	...workspaceManifests,
];
