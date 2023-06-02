import { UmbDocumentBlueprintStore } from './document-blueprint.detail.store.js';
import { UmbDocumentBlueprintTreeStore } from './document-blueprint.tree.store.js';
import { manifests as menuItemManifests } from './menu-item/manifests.js';
import { manifests as workspaceManifests } from './workspace/manifests.js';
import type { ManifestStore, ManifestTreeStore } from '@umbraco-cms/backoffice/extension-registry';

export const DOCUMENT_BLUEPRINT_STORE_ALIAS = 'Umb.Store.DocumentBlueprint';
export const DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS = 'Umb.Store.DocumentBlueprintTree';

const store: ManifestStore = {
	type: 'store',
	alias: DOCUMENT_BLUEPRINT_STORE_ALIAS,
	name: 'Document Blueprint Store',
	class: UmbDocumentBlueprintStore,
};

const treeStore: ManifestTreeStore = {
	type: 'treeStore',
	alias: DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
	name: 'Document Blueprint Tree Store',
	class: UmbDocumentBlueprintTreeStore,
};

export const manifests = [store, treeStore, ...menuItemManifests, ...workspaceManifests];
