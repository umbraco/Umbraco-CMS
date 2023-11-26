import { UmbDocumentBlueprintStore } from './document-blueprint.detail.store.js';
import { UmbDocumentBlueprintTreeStore } from './document-blueprint.tree.store.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestStore, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS = 'Umb.Store.DocumentBlueprint';
export const UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS = 'Umb.Store.DocumentBlueprintTree';

const store: ManifestStore = {
	type: 'store',
	alias: UMB_DOCUMENT_BLUEPRINT_STORE_ALIAS,
	name: 'Document Blueprint Store',
	api: UmbDocumentBlueprintStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
	name: 'Document Blueprint Tree Store',
	api: UmbDocumentBlueprintTreeStore,
};

export const manifests = [store, treeStore, ...menuItemManifests, ...workspaceManifests];
