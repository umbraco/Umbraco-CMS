import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../constants.js';
import { UMB_BLOCK_WORKSPACE_ALIAS } from '@umbraco-cms/backoffice/block';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.Document.AllowEditInvariantFromNonDefault',
		name: 'Allow Edit Invariant From NonDefault Document Controller',
		api: () => import('./document-workspace-allow-edit-invariant-from-non-default.controller.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.DocumentBlock.AllowEditInvariantFromNonDefault',
		name: 'Allow Edit Invariant From NonDefault Document Block Controller',
		api: () => import('./document-block-workspace-allow-edit-invariant-from-non-default.controller.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_BLOCK_WORKSPACE_ALIAS,
			},
		],
	},
];
