import { UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN } from '../repository/document-type.store';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentTypes',
	name: 'Document Types Tree',
	meta: {
		storeAlias: UMB_DOCUMENT_TYPE_STORE_CONTEXT_TOKEN.toString(),
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
