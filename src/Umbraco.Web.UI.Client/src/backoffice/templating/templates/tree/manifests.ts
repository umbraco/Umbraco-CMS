import { UmbTemplateRepository } from '../repository/template.repository';
import type { ManifestTree, ManifestTreeItem } from '@umbraco-cms/backoffice/extensions-registry';

const tree: ManifestTree = {
	type: 'tree',
	alias: 'Umb.Tree.Templates',
	name: 'Templates Tree',
	meta: {
		repository: UmbTemplateRepository,
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
