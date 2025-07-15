import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS } from '@umbraco-cms/backoffice/recycle-bin';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.Save',
		name: 'Save Document Workspace Action',
		weight: 80,
		api: () => import('./save.action.js'),
		meta: {
			label: '#buttons_save',
			look: 'secondary',
			color: 'positive',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
	{
		type: 'workspaceAction',
		kind: 'default',
		alias: 'Umb.WorkspaceAction.Document.SaveAndPreview',
		name: 'Save And Preview Document Workspace Action',
		weight: 90,
		api: () => import('./save-and-preview.action.js'),
		meta: {
			label: '#buttons_saveAndPreview',
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
			{
				alias: UMB_ENTITY_IS_NOT_TRASHED_CONDITION_ALIAS,
			},
		],
	},
];
