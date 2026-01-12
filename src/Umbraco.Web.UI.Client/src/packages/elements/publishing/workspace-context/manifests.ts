import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		name: 'Element Publishing Workspace Context',
		alias: 'Umb.WorkspaceContext.Element.Publishing',
		api: () => import('./element-publishing.workspace-context.js'),
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
