import { manifests as publishManifest } from './publish/manifests.js';
import { manifests as repositoryManifests } from './repository/manifests.js';
import { UMB_DOCUMENT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Document Publishing Workspace Context',
		alias: 'Umb.WorkspaceContext.Document.Publishing',
		api: () => import('./document-publishing.workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_DOCUMENT_WORKSPACE_ALIAS,
			},
		],
	},
	...repositoryManifests,
	...publishManifest,
];
