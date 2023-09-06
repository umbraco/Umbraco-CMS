import { SCRIPTS_ENTITY_TYPE, SCRIPTS_WORKSPACE_ACTION_SAVE_ALIAS, SCRIPTS_WORKSPACE_ALIAS } from '../config.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type { ManifestWorkspace, ManifestWorkspaceAction } from '@umbraco-cms/backoffice/extension-registry';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: SCRIPTS_WORKSPACE_ALIAS,
	name: 'Scripts Workspace',
	loader: () => import('./scripts-workspace.element.js'),
	meta: {
		entityType: SCRIPTS_ENTITY_TYPE,
	},
};

//TODO: this does not work for some reason
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.Scripts.Save',
		name: 'Save Script Workspace Action',
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
			api: UmbSaveWorkspaceAction,
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceActions];
