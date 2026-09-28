import { UMB_ELEMENT_WORKSPACE_ALIAS } from '../workspace/constants.js';
import { UmbContentWorkspaceAllowEditInvariantFromNonDefaultController } from '@umbraco-cms/backoffice/content';
import { UMB_WORKSPACE_CONDITION_ALIAS } from '@umbraco-cms/backoffice/workspace';

export const manifests: Array<UmbExtensionManifest> = [
	{
		type: 'workspaceContext',
		alias: 'Umb.WorkspaceContext.Element.AllowEditInvariantFromNonDefault',
		name: 'Allow Edit Invariant From NonDefault Element Controller',
		api: UmbContentWorkspaceAllowEditInvariantFromNonDefaultController,
		conditions: [
			{
				alias: UMB_WORKSPACE_CONDITION_ALIAS,
				match: UMB_ELEMENT_WORKSPACE_ALIAS,
			},
		],
	},
];
