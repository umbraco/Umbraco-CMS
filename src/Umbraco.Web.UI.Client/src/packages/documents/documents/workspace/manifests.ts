import { UMB_DOCUMENT_ENTITY_TYPE } from '../entity.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from './constants.js';
import { manifests as actionManifests } from './actions/manifests.js';
import { UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION } from '@umbraco-cms/backoffice/content';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspace',
		kind: 'routable',
		alias: UMB_DOCUMENT_WORKSPACE_ALIAS,
		name: 'Document Workspace',
		api: () => import('./document-workspace.context.js'),
		meta: {
			entityType: UMB_DOCUMENT_ENTITY_TYPE,
		},
	},
	{
		type: 'workspaceView',
		kind: 'contentEditor',
		alias: 'Umb.WorkspaceView.Document.Edit',
		name: 'Document Workspace Edit View',
		weight: 200,
		meta: {
			label: '#general_content',
			pathname: 'content',
			icon: 'document',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_CONTENT_HAS_PROPERTIES_WORKSPACE_CONDITION,
			},
		],
	},
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.Document.Info',
		name: 'Document Workspace Info View',
		element: () => import('./views/info/document-workspace-view-info.element.js'),
		weight: 100,
		meta: {
			label: '#general_info',
			pathname: 'info',
			icon: 'info',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
	...actionManifests,
];
