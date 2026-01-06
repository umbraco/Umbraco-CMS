import { UMB_ELEMENT_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../../collection/constants.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE } from '../entity.js';
import { UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'default',
		alias: UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS,
		name: 'Element Recycle Bin Root Workspace',
		meta: {
			entityType: UMB_ELEMENT_RECYCLE_BIN_ROOT_ENTITY_TYPE,
			headline: '#general_recycleBin',
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.ElementRecycleBinRoot.Root',
		name: 'Element Recycle Bin Root Collection Workspace View',
		meta: {
			label: 'Collection',
			pathname: 'collection',
			icon: 'icon-layers',
			collectionAlias: UMB_ELEMENT_RECYCLE_BIN_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_RECYCLE_BIN_ROOT_WORKSPACE_ALIAS,
			},
		],
	},
];
