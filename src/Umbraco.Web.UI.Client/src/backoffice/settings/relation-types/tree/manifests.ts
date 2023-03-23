import { UmbRelationTypeRepository } from '../repository/relation-type.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.RelationTypes',
	name: 'Relation Types Tree',
	meta: {
		repository: UmbRelationTypeRepository,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.RelationType',
	name: 'Relation Type Tree Item',
	conditions: {
		entityType: 'relation-type',
	},
};

export const manifests = [tree, treeItem];
