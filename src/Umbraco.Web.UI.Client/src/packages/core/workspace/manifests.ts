import { manifests as componentManifests } from './components/manifests.js';
import { manifests as workspaceKinds } from './kinds/manifests.js';
import { manifests as workspaceModals } from './modals/manifests.js';
import { manifests as workspaceConditions } from './conditions/manifests.js';
import type { ManifestTypes, UmbBackofficeManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<ManifestTypes | UmbBackofficeManifestKind> = [
	...workspaceConditions,
	...workspaceKinds,
	...componentManifests,
	...workspaceModals,
];
