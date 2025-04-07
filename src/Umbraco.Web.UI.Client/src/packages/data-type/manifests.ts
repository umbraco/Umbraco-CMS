import { manifests as collectionManifests } from './collection/manifests.js';
import { manifests as dataTypeRootManifest } from './data-type-root/manifests.js';
import { manifests as entityActions } from './entity-actions/manifests.js';
import { manifests as menuManifests } from './menu/manifests.js';
import { manifests as modalManifests } from './modals/manifests.js';
import { manifests as referenceManifests } from './reference/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { manifests as searchProviderManifests } from './search/manifests.js';
import { manifests as treeManifests } from './tree/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	...collectionManifests,
	...dataTypeRootManifest,
	...entityActions,
	...menuManifests,
	...modalManifests,
	...referenceManifests,
	...repositoryManifests,
	...searchProviderManifests,
	...treeManifests,
	...workspaceManifests,
];
