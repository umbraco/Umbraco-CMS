import type { UmbExtensionManifestKind } from '../extension-registry/registry.js';
import { manifests as conditionManifests } from './conditions/manifests.js';
import { manifests as actionManifests } from './action/manifests.js';
import { manifests as workspaceViewManifests } from './workspace-view/manifests.js';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...actionManifests,
	...workspaceViewManifests,
	...conditionManifests,
];
