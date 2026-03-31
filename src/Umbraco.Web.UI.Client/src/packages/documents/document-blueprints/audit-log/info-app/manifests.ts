import { UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_DOCUMENT_BLUEPRINT_AUDIT_LOG_REPOSITORY_ALIAS } from '../repository/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceInfoApp',
		kind: 'auditLog',
		name: 'Document Blueprint History Workspace Info App',
		alias: 'Umb.WorkspaceInfoApp.DocumentBlueprint.History',
		meta: {
			auditLogRepositoryAlias: UMB_DOCUMENT_BLUEPRINT_AUDIT_LOG_REPOSITORY_ALIAS,
		},
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_BLUEPRINT_WORKSPACE_ALIAS,
			},
		],
	},
];
