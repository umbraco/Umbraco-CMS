import { UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from './constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS, UmbSubmitWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
		name: 'Document Blueprint Workspace',
		api: () => import('./document-blueprint-workspace.context.js'),
		meta: {
			entityType: UMB_DOCUMENT_BLUEPRINT_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.DocumentBlueprint.Edit',
		name: 'Document Blueprint Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},

	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.DocumentBlueprint.Save',
		name: 'Save Document Workspace Action',
		weight: 80,
		api: UmbSubmitWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},
];
