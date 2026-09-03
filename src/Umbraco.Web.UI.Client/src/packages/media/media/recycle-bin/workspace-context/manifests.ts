import { UMB_MEDIA_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UmbTrashableMediaWorkspaceContext } from './trashable-media.workspace-context.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Trashable Media Workspace Context',
		alias: 'Umb.WorkspaceContext.Media.Trashable',
		api: UmbTrashableMediaWorkspaceContext,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_MEDIA_WORKSPACE_ALIAS,
			},
		],
	},
];
