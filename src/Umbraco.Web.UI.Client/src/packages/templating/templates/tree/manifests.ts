import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from './tree-item-children/constants.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { UmbTemplateTreeStore } from './template-tree.store.js';
import { UMB_TEMPLATE_TREE_REPOSITORY_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

/**
 * @deprecated Use {@link UMB_TEMPLATE_TREE_REPOSITORY_ALIAS} instead. This will be removed in Umbraco 18.
 */
export const UMB_TEMPLATE_TREE_STORE_ALIAS = 'Umb.Store.Template.Tree';
export const UMB_TEMPLATE_TREE_ALIAS = 'Umb.Tree.Template';

const UMB_TEMPLATE_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Template.Root';

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
		api: UmbTemplateTreeStore,
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
		alias: UMB_TEMPLATE_ROOT_WORKSPACE_ALIAS,
		name: 'Template Root Workspace',
		meta: {
			entityType: UMB_TEMPLATE_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_templates',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.Template.TreeItemChildrenCollection',
		name: 'Template Tree Item Children Collection Workspace View',
		meta: {
			label: '#general_items',
			pathname: 'items',
			icon: 'icon-grid',
			collectionAlias: UMB_TEMPLATE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_TEMPLATE_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
	...reloadTreeItemChildrenManifest,
	...treeItemChildrenManifests,
];
