import { UmbDocumentTypeRepository } from '../repository/document-type.repository';
import type { ManifestTree, ManifestTreeItemAction } from '@umbraco-cms/models';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.DocumentTypes',
	name: 'Document Types Tree',
	meta: {
		repository: UmbDocumentTypeRepository,
	},
};

const treeItemActions: Array<ManifestTreeItemAction> = [];

export const manifests = [tree, ...treeItemActions];
