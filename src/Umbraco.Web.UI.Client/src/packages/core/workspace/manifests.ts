import { manifests as componentManifests } from './components/manifests.js';
import { manifests as sectionRouteManifests } from './section-routes/manifests.js';
import { manifests as workspaceConditionManifests } from './conditions/manifests.js';
import { manifests as workspaceKindManifest } from './kinds/manifests.js';
import { manifests as workspaceModalManifest } from './modals/manifests.js';
import type { ManifestTypes, UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbExtensionManifestKind> = [
	...componentManifests,
	...sectionRouteManifests,
	...workspaceConditionManifests,
	...workspaceKindManifest,
	...workspaceModalManifest,
];
