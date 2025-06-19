import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';

export const UMB_TEMPLATE_TREE_REPOSITORY_ALIAS = 'Umb.Repository.Template.Tree';
export const UMB_TEMPLATE_TREE_STORE_ALIAS = 'Umb.Store.Template.Tree';
export const UMB_TEMPLATE_TREE_ALIAS = 'Umb.Tree.Template';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'repository',
		alias: UMB_TEMPLATE_TREE_REPOSITORY_ALIAS,
		name: 'Template Tree Repository',
		api: () => import('./template-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_TEMPLATE_TREE_STORE_ALIAS,
		name: 'Template Tree Store',
		api: () => import('./template-tree.store.js'),
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_TEMPLATE_TREE_ALIAS,
		name: 'Template Tree',
		meta: {
			repositoryAlias: UMB_TEMPLATE_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.Template',
		name: 'Template Tree Item',
		forEntityTypes: [UMB_TEMPLATE_ROOT_ENTITY_TYPE, UMB_TEMPLATE_ENTITY_TYPE],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: 'Umb.Workspace.Template.Root',
		name: 'Template Root Workspace',
		meta: {
			entityType: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_templates',
		},
	},
	...reloadTreeItemChildrenManifest,
];
