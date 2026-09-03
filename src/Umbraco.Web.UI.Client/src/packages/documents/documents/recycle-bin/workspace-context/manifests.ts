import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UmbTrashableDocumentWorkspaceContext } from './trashable-document.workspace-context.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Trashable Document Workspace Context',
		alias: 'Umb.WorkspaceContext.Document.Trashable',
		api: UmbTrashableDocumentWorkspaceContext,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
