import { UMB_USER_ENTITY_TYPE } from '../entity.js';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';
import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';

export const UMB_USER_WORKSPACE_ALIAS = 'Umb.Workspace.User';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: UMB_USER_WORKSPACE_ALIAS,
	name: 'User Workspace',
	element: () => import('./user-workspace.element.js'),
	meta: {
		entityType: UMB_USER_ENTITY_TYPE,
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];
const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.User.Save',
		name: 'Save User Workspace Action',
		api: UmbSaveWorkspaceAction,
		meta: {
			label: 'Save',
			look: 'primary',
			color: 'positive',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: workspace.alias,
			},
		],
	},
];

export const manifests = [workspace, ...workspaceViews, ...workspaceActions];
