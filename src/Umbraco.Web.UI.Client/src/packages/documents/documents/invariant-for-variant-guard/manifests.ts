import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../constants.js';
import { UmbContentWorkspaceInvariantForVariantGuardController } from '@umbraco-cms/backoffice/content';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.Document.InvariantForVariantGuard',
		name: 'Document Invariant For Variant Guard Workspace Context',
		api: UmbContentWorkspaceInvariantForVariantGuardController,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
