import { TEMPLATE_ENTITY_TYPE, TEMPLATE_ROOT_ENTITY_TYPE } from '../entities.js';
import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests.js';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extension-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		repositoryAlias: TEMPLATE_REPOSITORY_ALIAS,
	},
};

const treeItem: ManifestTreeItem = {
	type: 'treeItem',
	kind: 'entity',
	alias: 'Umb.TreeItem.Template',
	name: 'Template Tree Item',
	meta: {
		entityTypes: [TEMPLATE_ROOT_ENTITY_TYPE, TEMPLATE_ENTITY_TYPE],
	},
};

export const manifests = [tree, treeItem];
