import { UMB_TEMPLATE_ENTITY_TYPE, UMB_TEMPLATE_ROOT_ENTITY_TYPE } from '../entity.js';
import { manifests as reloadTreeItemChildrenManifest } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UmbTemplateTreeStore } from './template-tree.store.js';
import {
	UMB_TEMPLATE_TREE_ALIAS,
	UMB_TEMPLATE_TREE_REPOSITORY_ALIAS,
	UMB_TEMPLATE_TREE_STORE_ALIAS,
} from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export { UMB_TEMPLATE_TREE_REPOSITORY_ALIAS, UMB_TEMPLATE_TREE_STORE_ALIAS, UMB_TEMPLATE_TREE_ALIAS };

const UMB_TEMPLATE_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.Template.Root';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
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
		type: 'treeAction',
		kind: 'create',
		name: 'Template Tree Create Action',
		alias: 'Umb.TreeAction.Template.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_TEMPLATE_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.Template.Tree',
		name: 'Template Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_TEMPLATE_TREE_ALIAS,
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
	...viewManifests,
];
