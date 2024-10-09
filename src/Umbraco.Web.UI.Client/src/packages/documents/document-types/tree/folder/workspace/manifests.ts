import { UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE } from '../../../entity.js';
import { UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS } from '../../tree-item-children-collection/constants.js';
import { UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS } from './constants.js';
import { UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
		name: 'Document Type Folder Workspace',
		api: () => import('./document-type-folder-workspace.context.js'),
		meta: {
			entityType: UMB_DOCUMENT_TYPE_FOLDER_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		kind: 'collection',
		alias: 'Umb.WorkspaceView.DocumentType.Folder.Collection',
		name: 'Document Type Folder Collection Workspace View',
		meta: {
			label: '#general_design',
			pathname: 'design',
			icon: 'icon-member-dashed-line',
			collectionAlias: UMB_DOCUMENT_TYPE_TREE_ITEM_CHILDREN_COLLECTION_ALIAS,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DocumentType.Folder.Submit',
		name: 'Submit Document Type Folder Workspace Action',
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: '#buttons_save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_DOCUMENT_TYPE_FOLDER_WORKSPACE_ALIAS,
			},
		],
	},
];
