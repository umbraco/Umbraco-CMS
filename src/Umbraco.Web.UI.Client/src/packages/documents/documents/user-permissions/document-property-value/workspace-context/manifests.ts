import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'workspaceContext',
		name: 'Document Property Value User Permission Workspace Context',
		alias: 'Umb.WorkspaceContext.DocumentPropertyValueUserPermission',
		api: () => import('./document-property-value-user-permission.workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
