import {
	UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
	UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
} from '../entity.js';
import {
	UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
	UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
} from './constants.js';
import { UmbDocumentBlueprintTreeStore } from './document-blueprint-tree.store.js';
import { manifests as folderManifests } from './folder/manifests.js';
import { UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS } from './folder/workspace/constants.js';
import { manifests as reloadManifests } from './reload-tree-item-children/manifests.js';
import { manifests as treeItemChildrenManifests } from './tree-item-children/manifests.js';
import { manifests as viewManifests } from './views/manifests.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import { UMB_TREE_ALIAS_CONDITION } from '@umbraco-cms/backoffice/tree';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

const UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS = 'Umb.Workspace.DocumentBlueprint.Root';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'repository',
		alias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
		name: 'Document Blueprint Tree Repository',
		api: () => import('./document-blueprint-tree.repository.js'),
	},
	{
		type: 'treeStore',
		alias: UMB_DOCUMENT_BLUEPRINT_TREE_STORE_ALIAS,
		name: 'Document Blueprint Tree Store',
		api: UmbDocumentBlueprintTreeStore,
	},
	{
		type: 'tree',
		kind: 'default',
		alias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
		name: 'Document Blueprints Tree',
		meta: {
			repositoryAlias: UMB_DOCUMENT_BLUEPRINT_TREE_REPOSITORY_ALIAS,
		},
	},
	{
		type: 'treeItem',
		kind: 'default',
		alias: 'Umb.TreeItem.DocumentBlueprint',
		name: 'Document Blueprint Tree Item',
		forEntityTypes: [
			UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
			UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
			UMB_DOCUMENT_BLUEPRINT_FOLDER_ENTITY_TYPE,
		],
	},
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS,
		name: 'Document Blueprint Root Workspace',
		meta: {
			entityType: UMB_DOCUMENT_BLUEPRINT_ROOT_ENTITY_TYPE,
			headline: '#treeHeaders_contentBlueprints',
		},
	},
	{
		type: 'treeAction',
		kind: 'create',
		name: 'Document Blueprint Tree Create Action',
		alias: 'Umb.TreeAction.DocumentBlueprint.Create',
		conditions: [
			{
				alias: UMB_TREE_ALIAS_CONDITION,
				match: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceView',
		kind: 'tree',
		alias: 'Umb.WorkspaceView.DocumentBlueprint.Tree',
		name: 'Document Blueprint Tree Item Children Workspace View',
		meta: {
			label: '#tree_children',
			pathname: 'children',
			icon: 'icon-bulleted-list',
			treeAlias: UMB_DOCUMENT_BLUEPRINT_TREE_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				oneOf: [UMB_DOCUMENT_BLUEPRINT_ROOT_WORKSPACE_ALIAS, UMB_DOCUMENT_BLUEPRINT_FOLDER_WORKSPACE_ALIAS],
			},
		],
	},
	...reloadManifests,
	...folderManifests,
	...treeItemChildrenManifests,
	...viewManifests,
];
