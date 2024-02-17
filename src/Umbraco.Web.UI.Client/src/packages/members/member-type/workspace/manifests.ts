import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MemberType',
	name: 'Member Type Workspace',
	js: () => import('./member-type-workspace.element.js'),
	meta: {
		entityType: 'member-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [];

const workspaceActions: Array<ManifestWorkspaceAction> = [
	{
		type: 'workspaceAction',
		alias: 'Umb.WorkspaceAction.MemberType.Save',
		name: 'Save Member Type Workspace Action',
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
