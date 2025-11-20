import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';
import type { UmbExtensionManifestKind } from '@umbraco-cms/backoffice/extension-registry';
import { UMB_BLOCK_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/block';

export const manifests: Array<UmbExtensionManifest | UmbExtensionManifestKind> = [
	{
		type: 'workspaceContext',
		name: 'Document Property Value User Permission Document Workspace Context',
		alias: 'Umb.WorkspaceContext.Document.DocumentPropertyValueUserPermission',
		api: () => import('./document-property-value-user-permission.workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		name: 'Document Property Value User Permission Block Workspace Context',
		alias: 'Umb.WorkspaceContext.Block.DocumentPropertyValueUserPermission',
		api: () => import('./document-block-property-value-user-permission.workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
	},
];
