import { TEMPLATE_REPOSITORY_ALIAS } from '../repository/manifests';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

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
	conditions: {
		entityType: 'template',
	},
};

export const manifests = [tree, treeItem];
