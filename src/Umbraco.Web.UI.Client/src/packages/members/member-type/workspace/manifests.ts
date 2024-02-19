import type {
	ManifestWorkspace,
	ManifestWorkspaceAction,
	ManifestWorkspaceView,
} from '@umbraco-cms/backoffice/extension-registry';
import { UmbSaveWorkspaceAction } from '@umbraco-cms/backoffice/workspace';

export const UMB_MEMBER_TYPE_WORKSPACE_ALIAS = 'Umb.Workspace.MemberType';

const workspace: ManifestWorkspace = {
	type: 'workspace',
	alias: 'Umb.Workspace.MemberType',
	name: 'Member Type Workspace',
	js: () => import('./member-type-workspace.element.js'),
	meta: {
		entityType: 'member-type',
	},
};

const workspaceViews: Array<ManifestWorkspaceView> = [
	{
		type: 'workspaceView',
		alias: 'Umb.WorkspaceView.MemberType.Design',
		name: 'Member Type Workspace Design View',
		js: () => import('./views/design/member-type-workspace-view-edit.element.js'),
		weight: 1000,
		meta: {
			label: 'Design',
			pathname: 'design',
			icon: 'icon-member-dashed-line',
		},
		conditions: [
			{
				alias: 'Umb.Condition.WorkspaceAlias',
				match: UMB_MEMBER_TYPE_WORKSPACE_ALIAS,
			},
		],
	},
];

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
