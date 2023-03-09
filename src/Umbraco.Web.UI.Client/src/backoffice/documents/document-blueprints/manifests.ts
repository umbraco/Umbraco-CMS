import { UmbDocumentBlueprintStore } from './document-blueprint.detail.store';
import { UmbDocumentBlueprintTreeStore } from './document-blueprint.tree.store';
import { manifests as menuItemManifests } from './menu-item/manifests';
import { manifests as workspaceManifests } from './workspace/manifests';
import type { ManifestStore, ManifestTreeStore } from '@umbraco-cms/extensions-registry';

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
