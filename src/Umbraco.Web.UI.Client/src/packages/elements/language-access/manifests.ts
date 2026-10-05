import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		kind: 'contentLanguageAccess',
		name: 'Element Language Access Workspace Context',
		alias: 'Umb.WorkspaceContext.Element.LanguageAccess',
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
